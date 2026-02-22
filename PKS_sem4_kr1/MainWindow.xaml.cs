using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;
using PKS_sem4_kr1.Views;
using System.Collections.Generic;

namespace PKS_sem4_kr1
{
    public partial class MainWindow : Window
    {
        private LibraryContext _context;
        
        public MainWindow()
        {
            InitializeComponent();
            _context = new LibraryContext();
            
            // Убеждаемся, что база данных создана
            _context.Database.EnsureCreated();
            
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка книг с авторами и жанрами
                _context.Books.Include(b => b.Author).Include(b => b.Genre).Load();
                BooksDataGrid.ItemsSource = _context.Books.Local.ToObservableCollection();
                
                // Загрузка авторов и жанров для фильтров
                _context.Authors.Load();
                _context.Genres.Load();
                
                // Настройка ComboBox для фильтров
                SetupFilterComboBoxes();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupFilterComboBoxes()
        {
            // Авторы
            var authorsList = _context.Authors.Local.ToObservableCollection();
            var authorItems = new List<object>();
            authorItems.Add(new { Id = 0, FullName = "Все авторы" });
            foreach (var author in authorsList)
            {
                authorItems.Add(author);
            }
            AuthorFilter.ItemsSource = authorItems;
            AuthorFilter.SelectedIndex = 0;

            // Жанры
            var genresList = _context.Genres.Local.ToObservableCollection();
            var genreItems = new List<object>();
            genreItems.Add(new { Id = 0, Name = "Все жанры" });
            foreach (var genre in genresList)
            {
                genreItems.Add(genre);
            }
            GenreFilter.ItemsSource = genreItems;
            GenreFilter.SelectedIndex = 0;
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            var window = new BookWindow(_context);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                RefreshData();
            }
        }

        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book selectedBook)
            {
                var window = new BookWindow(_context, selectedBook);
                window.Owner = this;
                if (window.ShowDialog() == true)
                {
                    RefreshData();
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для редактирования", "Информация", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book selectedBook)
            {
                var result = MessageBox.Show($"Удалить книгу '{selectedBook.Title}'?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Books.Remove(selectedBook);
                        _context.SaveChanges();
                        RefreshData();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления", "Информация", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ManageAuthors_Click(object sender, RoutedEventArgs e)
        {
            var window = new AuthorsWindow(_context);
            window.Owner = this;
            window.ShowDialog();
            RefreshData();
        }

        private void ManageGenres_Click(object sender, RoutedEventArgs e)
        {
            var window = new GenresWindow(_context);
            window.Owner = this;
            window.ShowDialog();
            RefreshData();
        }

        private void RefreshData()
        {
            // Очищаем локальный кэш и загружаем заново
            _context.ChangeTracker.Clear();
            LoadData();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBooks();
        }

        private void AuthorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterBooks();
        }

        private void GenreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterBooks();
        }

        private void FilterBooks()
        {
            try
            {
                var query = _context.Books
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .AsQueryable();

                // Фильтр по названию
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    query = query.Where(b => b.Title.Contains(SearchBox.Text));
                }

                // Фильтр по автору
                if (AuthorFilter.SelectedItem is Author selectedAuthor)
                {
                    query = query.Where(b => b.AuthorId == selectedAuthor.Id);
                }

                // Фильтр по жанру
                if (GenreFilter.SelectedItem is Genre selectedGenre)
                {
                    query = query.Where(b => b.GenreId == selectedGenre.Id);
                }

                BooksDataGrid.ItemsSource = query.ToList();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
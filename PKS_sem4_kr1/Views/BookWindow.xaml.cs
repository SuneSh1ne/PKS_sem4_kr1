using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;

namespace PKS_sem4_kr1.Views
{
    public partial class BookWindow : Window
    {
        private LibraryContext _context;
        private Book _book;

        public BookWindow(LibraryContext context, Book book = null)
        {
            InitializeComponent();
            _context = context;
            _book = book ?? new Book();

            // Загрузка авторов и жанров
            _context.Authors.Load();
            _context.Genres.Load();
            
            AuthorBox.ItemsSource = _context.Authors.Local.ToObservableCollection();
            GenreBox.ItemsSource = _context.Genres.Local.ToObservableCollection();

            if (book != null)
            {
                Title = "Редактирование книги";
                LoadBookData();
            }
            else
            {
                Title = "Добавление книги";
            }
        }

        private void LoadBookData()
        {
            TitleBox.Text = _book.Title;
            AuthorBox.SelectedItem = _book.Author;
            GenreBox.SelectedItem = _book.Genre;
            YearBox.Text = _book.PublishYear.ToString();
            ISBNBox.Text = _book.ISBN;
            QuantityBox.Text = _book.QuantityInStock.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(TitleBox.Text))
                {
                    MessageBox.Show("Введите название книги");
                    return;
                }

                if (AuthorBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите автора");
                    return;
                }

                if (GenreBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите жанр");
                    return;
                }

                if (!int.TryParse(YearBox.Text, out int year) || year < 1000 || year > 2100)
                {
                    MessageBox.Show("Введите корректный год (1000-2100)");
                    return;
                }

                if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity < 0)
                {
                    MessageBox.Show("Введите корректное количество (неотрицательное число)");
                    return;
                }

                // Сохранение данных
                _book.Title = TitleBox.Text.Trim();
                _book.AuthorId = ((Author)AuthorBox.SelectedItem).Id;
                _book.GenreId = ((Genre)GenreBox.SelectedItem).Id;
                _book.PublishYear = year;
                _book.ISBN = ISBNBox.Text?.Trim();
                _book.QuantityInStock = quantity;

                if (_book.Id == 0)
                {
                    _context.Books.Add(_book);
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
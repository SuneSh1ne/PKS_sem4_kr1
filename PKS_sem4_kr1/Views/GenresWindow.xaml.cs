using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;

namespace PKS_sem4_kr1.Views
{
    public partial class GenresWindow : Window
    {
        private LibraryContext _context;
        private Genre _currentGenre;
        private bool _isEditMode;

        public GenresWindow(LibraryContext context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            _context.Genres.Include(g => g.Books).Load();
            GenresDataGrid.ItemsSource = _context.Genres.Local.ToObservableCollection();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterGenres();
        }

        private void FilterGenres()
        {
            var query = _context.Genres.Include(g => g.Books).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                var searchText = SearchBox.Text.ToLower();
                query = query.Where(g => g.Name.ToLower().Contains(searchText) ||
                                        (g.Description != null && g.Description.ToLower().Contains(searchText)));
            }

            GenresDataGrid.ItemsSource = query.ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentGenre = new Genre();
            _isEditMode = true;
            ShowEditPanel();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (GenresDataGrid.SelectedItem is Genre selectedGenre)
            {
                _currentGenre = new Genre
                {
                    Id = selectedGenre.Id,
                    Name = selectedGenre.Name,
                    Description = selectedGenre.Description
                };
                _isEditMode = true;
                ShowEditPanel();
            }
            else
            {
                MessageBox.Show("Выберите жанр для редактирования", "Информация", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (GenresDataGrid.SelectedItem is Genre selectedGenre)
            {
                // Проверяем, есть ли у жанра книги
                if (selectedGenre.Books != null && selectedGenre.Books.Any())
                {
                    MessageBox.Show("Нельзя удалить жанр, к которому относятся книги", "Ошибка", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Удалить жанр '{selectedGenre.Name}'?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Genres.Remove(selectedGenre);
                        _context.SaveChanges();
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", 
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите жанр для удаления", "Информация", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowEditPanel()
        {
            if (_currentGenre != null)
            {
                NameBox.Text = _currentGenre.Name;
                DescriptionBox.Text = _currentGenre.Description;

                EditPanel.Visibility = Visibility.Visible;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(NameBox.Text))
                {
                    MessageBox.Show("Введите название жанра");
                    return;
                }

                // Сохранение данных
                _currentGenre.Name = NameBox.Text.Trim();
                _currentGenre.Description = DescriptionBox.Text?.Trim();

                if (_currentGenre.Id == 0)
                {
                    _context.Genres.Add(_currentGenre);
                }
                else
                {
                    var genre = _context.Genres.Find(_currentGenre.Id);
                    if (genre != null)
                    {
                        genre.Name = _currentGenre.Name;
                        genre.Description = _currentGenre.Description;
                    }
                }

                _context.SaveChanges();
                LoadData();
                CancelEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
        }

        private void CancelEdit()
        {
            _currentGenre = null;
            _isEditMode = false;
            EditPanel.Visibility = Visibility.Collapsed;
            
            // Очищаем поля
            NameBox.Text = "";
            DescriptionBox.Text = "";
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Здесь можно добавить валидацию в реальном времени
        }

        private void GenresDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Отключаем режим редактирования при смене выделения
            if (!_isEditMode)
            {
                CancelEdit();
            }
        }
    }
}
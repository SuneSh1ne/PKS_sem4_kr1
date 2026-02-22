using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;

namespace PKS_sem4_kr1.Views
{
    public partial class AuthorsWindow : Window
    {
        private LibraryContext _context;
        private Author _currentAuthor;
        private bool _isEditMode;

        public AuthorsWindow(LibraryContext context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            _context.Authors.Include(a => a.Books).Load();
            AuthorsDataGrid.ItemsSource = _context.Authors.Local.ToObservableCollection();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAuthors();
        }

        private void FilterAuthors()
        {
            var query = _context.Authors.Include(a => a.Books).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                var searchText = SearchBox.Text.ToLower();
                query = query.Where(a => a.FirstName.ToLower().Contains(searchText) ||
                                        a.LastName.ToLower().Contains(searchText) ||
                                        (a.Country != null && a.Country.ToLower().Contains(searchText)));
            }

            AuthorsDataGrid.ItemsSource = query.ToList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentAuthor = new Author();
            _isEditMode = true;
            ShowEditPanel();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorsDataGrid.SelectedItem is Author selectedAuthor)
            {
                _currentAuthor = new Author
                {
                    Id = selectedAuthor.Id,
                    FirstName = selectedAuthor.FirstName,
                    LastName = selectedAuthor.LastName,
                    BirthDate = selectedAuthor.BirthDate,
                    Country = selectedAuthor.Country
                };
                _isEditMode = true;
                ShowEditPanel();
            }
            else
            {
                MessageBox.Show("Выберите автора для редактирования", "Информация", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorsDataGrid.SelectedItem is Author selectedAuthor)
            {
                // Проверяем, есть ли у автора книги
                if (selectedAuthor.Books != null && selectedAuthor.Books.Any())
                {
                    MessageBox.Show("Нельзя удалить автора, у которого есть книги", "Ошибка", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Удалить автора {selectedAuthor.FullName}?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Authors.Remove(selectedAuthor);
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
                MessageBox.Show("Выберите автора для удаления", "Информация", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowEditPanel()
        {
            if (_currentAuthor != null)
            {
                FirstNameBox.Text = _currentAuthor.FirstName;
                LastNameBox.Text = _currentAuthor.LastName;
                BirthDatePicker.SelectedDate = _currentAuthor.BirthDate;
                CountryBox.Text = _currentAuthor.Country;

                EditPanel.Visibility = Visibility.Visible;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
                {
                    MessageBox.Show("Введите имя автора");
                    return;
                }

                if (string.IsNullOrWhiteSpace(LastNameBox.Text))
                {
                    MessageBox.Show("Введите фамилию автора");
                    return;
                }

                // Сохранение данных
                _currentAuthor.FirstName = FirstNameBox.Text.Trim();
                _currentAuthor.LastName = LastNameBox.Text.Trim();
                _currentAuthor.BirthDate = BirthDatePicker.SelectedDate ?? DateTime.Now;
                _currentAuthor.Country = CountryBox.Text?.Trim();

                if (_currentAuthor.Id == 0)
                {
                    _context.Authors.Add(_currentAuthor);
                }
                else
                {
                    var author = _context.Authors.Find(_currentAuthor.Id);
                    if (author != null)
                    {
                        author.FirstName = _currentAuthor.FirstName;
                        author.LastName = _currentAuthor.LastName;
                        author.BirthDate = _currentAuthor.BirthDate;
                        author.Country = _currentAuthor.Country;
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
            _currentAuthor = null;
            _isEditMode = false;
            EditPanel.Visibility = Visibility.Collapsed;
            
            // Очищаем поля
            FirstNameBox.Text = "";
            LastNameBox.Text = "";
            BirthDatePicker.SelectedDate = null;
            CountryBox.Text = "";
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Здесь можно добавить валидацию в реальном времени
        }

        private void AuthorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Отключаем режим редактирования при смене выделения
            if (!_isEditMode)
            {
                CancelEdit();
            }
        }
    }
}
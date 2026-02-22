using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;

namespace PKS_sem4_kr1.ViewModels
{
    public class AuthorViewModel : INotifyPropertyChanged
    {
        private LibraryContext _context;
        private ObservableCollection<Author> _authors;
        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public AuthorViewModel(LibraryContext context)
        {
            _context = context;
            LoadData();
            
            AddCommand = new RelayCommand(AddAuthor);
            EditCommand = new RelayCommand(EditAuthor, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteAuthor, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveAuthor, CanSave);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<Author> Authors
        {
            get { return _authors; }
            set
            {
                _authors = value;
                OnPropertyChanged(nameof(Authors));
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterAuthors();
            }
        }

        public Author SelectedAuthor { get; set; }
        public Author CurrentAuthor { get; set; }
        public bool IsEditMode { get; set; }

        public ICommand AddCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void LoadData()
        {
            _context.Authors.Load();
            Authors = _context.Authors.Local.ToObservableCollection();
        }

        private void FilterAuthors()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Authors = _context.Authors.Local.ToObservableCollection();
            }
            else
            {
                var filtered = _context.Authors.Local
                    .Where(a => a.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                a.Country.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                Authors = new ObservableCollection<Author>(filtered);
            }
        }

        private void AddAuthor(object parameter)
        {
            CurrentAuthor = new Author();
            IsEditMode = true;
            OnPropertyChanged(nameof(CurrentAuthor));
            OnPropertyChanged(nameof(IsEditMode));
        }

        private void EditAuthor(object parameter)
        {
            if (SelectedAuthor != null)
            {
                CurrentAuthor = new Author
                {
                    Id = SelectedAuthor.Id,
                    FirstName = SelectedAuthor.FirstName,
                    LastName = SelectedAuthor.LastName,
                    BirthDate = SelectedAuthor.BirthDate,
                    Country = SelectedAuthor.Country
                };
                IsEditMode = true;
                OnPropertyChanged(nameof(CurrentAuthor));
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private void DeleteAuthor(object parameter)
        {
            if (SelectedAuthor != null)
            {
                try
                {
                    // Проверяем, есть ли у автора книги
                    var hasBooks = _context.Books.Any(b => b.AuthorId == SelectedAuthor.Id);
                    if (hasBooks)
                    {
                        throw new InvalidOperationException("Нельзя удалить автора, у которого есть книги");
                    }

                    _context.Authors.Remove(SelectedAuthor);
                    _context.SaveChanges();
                    LoadData();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка при удалении автора: {ex.Message}");
                }
            }
        }

        private void SaveAuthor(object parameter)
        {
            try
            {
                if (CurrentAuthor.Id == 0)
                {
                    _context.Authors.Add(CurrentAuthor);
                }
                else
                {
                    var author = _context.Authors.Find(CurrentAuthor.Id);
                    if (author != null)
                    {
                        author.FirstName = CurrentAuthor.FirstName;
                        author.LastName = CurrentAuthor.LastName;
                        author.BirthDate = CurrentAuthor.BirthDate;
                        author.Country = CurrentAuthor.Country;
                    }
                }
                _context.SaveChanges();
                LoadData();
                CancelEdit(null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении автора: {ex.Message}");
            }
        }

        private void CancelEdit(object parameter)
        {
            CurrentAuthor = null;
            IsEditMode = false;
            OnPropertyChanged(nameof(CurrentAuthor));
            OnPropertyChanged(nameof(IsEditMode));
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedAuthor != null;
        }

        private bool CanSave(object parameter)
        {
            return CurrentAuthor != null && 
                   !string.IsNullOrWhiteSpace(CurrentAuthor.FirstName) &&
                   !string.IsNullOrWhiteSpace(CurrentAuthor.LastName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
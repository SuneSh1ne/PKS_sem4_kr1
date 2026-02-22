using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;

namespace PKS_sem4_kr1.ViewModels
{
    public class GenreViewModel : INotifyPropertyChanged
    {
        private LibraryContext _context;
        private ObservableCollection<Genre> _genres;
        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public GenreViewModel(LibraryContext context)
        {
            _context = context;
            LoadData();
            
            AddCommand = new RelayCommand(AddGenre);
            EditCommand = new RelayCommand(EditGenre, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteGenre, CanEditOrDelete);
            SaveCommand = new RelayCommand(SaveGenre, CanSave);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        public ObservableCollection<Genre> Genres
        {
            get { return _genres; }
            set
            {
                _genres = value;
                OnPropertyChanged(nameof(Genres));
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterGenres();
            }
        }

        public Genre SelectedGenre { get; set; }
        public Genre CurrentGenre { get; set; }
        public bool IsEditMode { get; set; }

        public ICommand AddCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void LoadData()
        {
            _context.Genres.Load();
            Genres = _context.Genres.Local.ToObservableCollection();
        }

        private void FilterGenres()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Genres = _context.Genres.Local.ToObservableCollection();
            }
            else
            {
                var filtered = _context.Genres.Local
                    .Where(g => g.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                               (g.Description != null && g.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                Genres = new ObservableCollection<Genre>(filtered);
            }
        }

        private void AddGenre(object parameter)
        {
            CurrentGenre = new Genre();
            IsEditMode = true;
            OnPropertyChanged(nameof(CurrentGenre));
            OnPropertyChanged(nameof(IsEditMode));
        }

        private void EditGenre(object parameter)
        {
            if (SelectedGenre != null)
            {
                CurrentGenre = new Genre
                {
                    Id = SelectedGenre.Id,
                    Name = SelectedGenre.Name,
                    Description = SelectedGenre.Description
                };
                IsEditMode = true;
                OnPropertyChanged(nameof(CurrentGenre));
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        private void DeleteGenre(object parameter)
        {
            if (SelectedGenre != null)
            {
                try
                {
                    // Проверяем, есть ли у жанра книги
                    var hasBooks = _context.Books.Any(b => b.GenreId == SelectedGenre.Id);
                    if (hasBooks)
                    {
                        throw new InvalidOperationException("Нельзя удалить жанр, к которому относятся книги");
                    }

                    _context.Genres.Remove(SelectedGenre);
                    _context.SaveChanges();
                    LoadData();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка при удалении жанра: {ex.Message}");
                }
            }
        }

        private void SaveGenre(object parameter)
        {
            try
            {
                if (CurrentGenre.Id == 0)
                {
                    _context.Genres.Add(CurrentGenre);
                }
                else
                {
                    var genre = _context.Genres.Find(CurrentGenre.Id);
                    if (genre != null)
                    {
                        genre.Name = CurrentGenre.Name;
                        genre.Description = CurrentGenre.Description;
                    }
                }
                _context.SaveChanges();
                LoadData();
                CancelEdit(null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении жанра: {ex.Message}");
            }
        }

        private void CancelEdit(object parameter)
        {
            CurrentGenre = null;
            IsEditMode = false;
            OnPropertyChanged(nameof(CurrentGenre));
            OnPropertyChanged(nameof(IsEditMode));
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedGenre != null;
        }

        private bool CanSave(object parameter)
        {
            return CurrentGenre != null && !string.IsNullOrWhiteSpace(CurrentGenre.Name);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
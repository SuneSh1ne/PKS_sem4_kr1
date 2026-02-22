using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PKS_sem4_kr1.Models;

namespace PKS_sem4_kr1.ViewModels
{
    public class BookViewModel : INotifyPropertyChanged
    {
        private LibraryContext _context;
        private ObservableCollection<Book> _books;
        private ObservableCollection<Author> _authors;
        private ObservableCollection<Genre> _genres;
        private string _searchText;
        private Author _selectedAuthor;
        private Genre _selectedGenre;

        public event PropertyChangedEventHandler PropertyChanged;

        public BookViewModel()
        {
            _context = new LibraryContext();
            LoadData();
            
            AddCommand = new RelayCommand(AddBook);
            EditCommand = new RelayCommand(EditBook, CanEditOrDelete);
            DeleteCommand = new RelayCommand(DeleteBook, CanEditOrDelete);
        }

        public ObservableCollection<Book> Books
        {
            get { return _books; }
            set
            {
                _books = value;
                OnPropertyChanged(nameof(Books));
            }
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
                FilterBooks();
            }
        }

        public Author SelectedAuthor
        {
            get { return _selectedAuthor; }
            set
            {
                _selectedAuthor = value;
                OnPropertyChanged(nameof(SelectedAuthor));
                FilterBooks();
            }
        }

        public Genre SelectedGenre
        {
            get { return _selectedGenre; }
            set
            {
                _selectedGenre = value;
                OnPropertyChanged(nameof(SelectedGenre));
                FilterBooks();
            }
        }

        public Book SelectedBook { get; set; }

        public ICommand AddCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        private void LoadData()
        {
            _context.Books.Load();
            _context.Authors.Load();
            _context.Genres.Load();
            
            Books = _context.Books.Local.ToObservableCollection();
            Authors = _context.Authors.Local.ToObservableCollection();
            Genres = _context.Genres.Local.ToObservableCollection();
        }

        private void FilterBooks()
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(b => b.Title.Contains(SearchText));
            }

            if (SelectedAuthor != null)
            {
                query = query.Where(b => b.AuthorId == SelectedAuthor.Id);
            }

            if (SelectedGenre != null)
            {
                query = query.Where(b => b.GenreId == SelectedGenre.Id);
            }

            Books = new ObservableCollection<Book>(query.ToList());
        }

        private void AddBook(object parameter)
        {
            var book = new Book();
            // Открыть окно добавления книги
        }

        private void EditBook(object parameter)
        {
            if (SelectedBook != null)
            {
                // Открыть окно редактирования книги
            }
        }

        private void DeleteBook(object parameter)
        {
            if (SelectedBook != null)
            {
                _context.Books.Remove(SelectedBook);
                _context.SaveChanges();
                LoadData();
            }
        }

        private bool CanEditOrDelete(object parameter)
        {
            return SelectedBook != null;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
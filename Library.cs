using System;
using System.Collections.Generic;
using System.Linq;

namespace LibrarySystem
{
    public class Book
    {
        public string Title  { get; set; }
        public string Author { get; set; }
        public int    Year   { get; set; }
        public bool   IsAvailable { get; private set; }

        public Book(string title, string author, int year)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва книги не може бути порожньою.", nameof(title));
            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("Автор книги не може бути порожнім.", nameof(author));
            if (year < 1 || year > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException(nameof(year),
                    $"Рік має бути від 1 до {DateTime.Now.Year}.");

            Title       = title;
            Author      = author;
            Year        = year;
            IsAvailable = true;
        }

        public void Borrow() => IsAvailable = false;
        public void Return() => IsAvailable = true;

        public override string ToString() =>
            $"\"{Title}\" — {Author} ({Year}) [{(IsAvailable ? "доступна" : "видана")}]";
    }

    public class Library
    {
        private readonly List<Book> _books = new();
        private readonly int        _maxCapacity;

        public Library(int maxCapacity = 100)
        {
            if (maxCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity),
                    "Місткість бібліотеки має бути більше 0.");
            _maxCapacity = maxCapacity;
        }

        public IReadOnlyList<Book> Books => _books.AsReadOnly();

        public void AddBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book), "Книга не може бути null.");

            if (_books.Count >= _maxCapacity)
                throw new InvalidOperationException(
                    $"Бібліотека переповнена (максимум {_maxCapacity} книг).");

            bool duplicate = _books.Any(b =>
                b.Title.Equals(book.Title, StringComparison.OrdinalIgnoreCase) &&
                b.Author.Equals(book.Author, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
                throw new InvalidOperationException(
                    $"Книга \"{book.Title}\" автора {book.Author} вже є в бібліотеці.");

            _books.Add(book);
        }

        public Book BorrowBook(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва для пошуку не може бути порожньою.", nameof(title));

            Book? found = _books.FirstOrDefault(b =>
                b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (found == null)
                throw new KeyNotFoundException($"Книгу \"{title}\" не знайдено в бібліотеці.");

            if (!found.IsAvailable)
                throw new InvalidOperationException($"Книга \"{title}\" вже видана іншому читачу.");

            found.Borrow();
            return found;
        }

        public List<Book> FindBooks(string? author = null, int? fromYear = null, int? toYear = null)
        {
            if (author == null && fromYear == null && toYear == null)
                throw new ArgumentException("Необхідно вказати хоча б один критерій пошуку.");

            if (fromYear.HasValue && toYear.HasValue && fromYear > toYear)
                throw new ArgumentException("fromYear не може бути більшим за toYear.");

            var result = _books.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(author))
                result = result.Where(b =>
                    b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

            if (fromYear.HasValue)
                result = result.Where(b => b.Year >= fromYear.Value);

            if (toYear.HasValue)
                result = result.Where(b => b.Year <= toYear.Value);

            return result.ToList();
        }

        public LibraryStatistics GetStatistics()
        {
            int total     = _books.Count;
            int available = _books.Count(b => b.IsAvailable);
            int borrowed  = total - available;

            double avgYear = total > 0
                ? _books.Average(b => b.Year)
                : 0;

            string oldestTitle = total > 0
                ? _books.OrderBy(b => b.Year).First().Title
                : string.Empty;

            return new LibraryStatistics(total, available, borrowed, avgYear, oldestTitle);
        }

        public void ReturnBook(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва не може бути порожньою.", nameof(title));

            Book? found = _books.FirstOrDefault(b =>
                b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (found == null)
                throw new KeyNotFoundException($"Книгу \"{title}\" не знайдено в бібліотеці.");

            if (found.IsAvailable)
                throw new InvalidOperationException($"Книга \"{title}\" не була видана.");

            found.Return();
        }
    }

    public record LibraryStatistics(
        int    Total,
        int    Available,
        int    Borrowed,
        double AverageYear,
        string OldestBookTitle);
}

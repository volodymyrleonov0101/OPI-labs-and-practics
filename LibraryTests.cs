using System;
using System.Collections.Generic;
using Xunit;
using LibrarySystem;

namespace LibrarySystem.Tests
{
    public class BookTests
    {
        [Fact]
        public void Book_ValidData_CreatesSuccessfully()
        {
            string title  = "Кобзар";
            string author = "Тарас Шевченко";
            int    year   = 1840;

            var book = new Book(title, author, year);

            Assert.Equal(title,  book.Title);
            Assert.Equal(author, book.Author);
            Assert.Equal(year,   book.Year);
            Assert.True(book.IsAvailable);
        }

        [Fact]
        public void Book_EmptyTitle_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Book("", "Автор", 2000));
        }

        [Fact]
        public void Book_EmptyAuthor_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Book("Назва", "   ", 2000));
        }

        [Fact]
        public void Book_YearZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Book("Назва", "Автор", 0));
        }

        [Fact]
        public void Book_YearOne_CreatesSuccessfully()
        {
            var book = new Book("Назва", "Автор", 1);

            Assert.Equal(1, book.Year);
        }

        [Fact]
        public void Book_CurrentYear_CreatesSuccessfully()
        {
            int currentYear = DateTime.Now.Year;

            var book = new Book("Назва", "Автор", currentYear);

            Assert.Equal(currentYear, book.Year);
        }

        [Fact]
        public void Book_FutureYear_ThrowsArgumentOutOfRangeException()
        {
            int futureYear = DateTime.Now.Year + 1;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Book("Назва", "Автор", futureYear));
        }
    }

    public class LibraryAddBookTests
    {
        [Fact]
        public void AddBook_ValidBook_BookAppearsInCollection()
        {
            var library = new Library();
            var book    = new Book("1984", "Джордж Орвелл", 1949);

            library.AddBook(book);

            Assert.Contains(book, library.Books);
        }

        [Fact]
        public void AddBook_NullBook_ThrowsArgumentNullException()
        {
            var library = new Library();

            Assert.Throws<ArgumentNullException>(() =>
                library.AddBook(null!));
        }

        [Fact]
        public void AddBook_DuplicateBook_ThrowsInvalidOperationException()
        {
            var library = new Library();
            var book1   = new Book("1984", "Джордж Орвелл", 1949);
            var book2   = new Book("1984", "Джордж Орвелл", 1949);
            library.AddBook(book1);

            Assert.Throws<InvalidOperationException>(() =>
                library.AddBook(book2));
        }

        [Fact]
        public void AddBook_ExceedsCapacity_ThrowsInvalidOperationException()
        {
            var library = new Library(maxCapacity: 1);
            library.AddBook(new Book("Книга А", "Автор А", 2000));

            Assert.Throws<InvalidOperationException>(() =>
                library.AddBook(new Book("Книга Б", "Автор Б", 2001)));
        }

        [Fact]
        public void AddBook_ExactlyAtCapacity_Succeeds()
        {
            var library = new Library(maxCapacity: 2);

            library.AddBook(new Book("Книга А", "Автор А", 2000));
            library.AddBook(new Book("Книга Б", "Автор Б", 2001));

            Assert.Equal(2, library.Books.Count);
        }
    }

    public class LibraryBorrowBookTests
    {
        [Fact]
        public void BorrowBook_AvailableBook_ReturnsBookAndMarksUnavailable()
        {
            var library = new Library();
            var book    = new Book("Майстер і Маргарита", "Булгаков", 1966);
            library.AddBook(book);

            var borrowed = library.BorrowBook("Майстер і Маргарита");

            Assert.NotNull(borrowed);
            Assert.False(borrowed.IsAvailable);
        }

        [Fact]
        public void BorrowBook_AlreadyBorrowed_ThrowsInvalidOperationException()
        {
            var library = new Library();
            library.AddBook(new Book("Майстер і Маргарита", "Булгаков", 1966));
            library.BorrowBook("Майстер і Маргарита");

            Assert.Throws<InvalidOperationException>(() =>
                library.BorrowBook("Майстер і Маргарита"));
        }

        [Fact]
        public void BorrowBook_NonExistentTitle_ThrowsKeyNotFoundException()
        {
            var library = new Library();

            Assert.Throws<KeyNotFoundException>(() =>
                library.BorrowBook("Неіснуюча книга"));
        }

        [Fact]
        public void BorrowBook_EmptyTitle_ThrowsArgumentException()
        {
            var library = new Library();

            Assert.Throws<ArgumentException>(() =>
                library.BorrowBook(""));
        }
    }

    public class LibraryFindBooksTests
    {
        private Library BuildLibrary()
        {
            var lib = new Library();
            lib.AddBook(new Book("Кобзар",               "Шевченко",      1840));
            lib.AddBook(new Book("Лісова пісня",         "Леся Українка", 1911));
            lib.AddBook(new Book("Тіні забутих предків", "Коцюбинський",  1911));
            lib.AddBook(new Book("1984",                 "Орвелл",        1949));
            return lib;
        }

        [Fact]
        public void FindBooks_ByAuthor_ReturnsMatchingBooks()
        {
            var library = BuildLibrary();

            var result = library.FindBooks(author: "Шевченко");

            Assert.Single(result);
            Assert.Equal("Кобзар", result[0].Title);
        }

        [Fact]
        public void FindBooks_ByExactYear_ReturnsBooksOfThatYear()
        {
            var library = BuildLibrary();

            var result = library.FindBooks(fromYear: 1911, toYear: 1911);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FindBooks_NoParameters_ThrowsArgumentException()
        {
            var library = BuildLibrary();

            Assert.Throws<ArgumentException>(() =>
                library.FindBooks());
        }

        [Fact]
        public void FindBooks_FromYearGreaterThanToYear_ThrowsArgumentException()
        {
            var library = BuildLibrary();

            Assert.Throws<ArgumentException>(() =>
                library.FindBooks(fromYear: 2000, toYear: 1900));
        }

        [Fact]
        public void FindBooks_AuthorNotFound_ReturnsEmptyList()
        {
            var library = BuildLibrary();

            var result = library.FindBooks(author: "Невідомий автор");

            Assert.Empty(result);
        }
    }

    public class LibraryStatisticsTests
    {
        [Fact]
        public void GetStatistics_EmptyLibrary_ReturnsZeros()
        {
            var library = new Library();

            var stats = library.GetStatistics();

            Assert.Equal(0, stats.Total);
            Assert.Equal(0, stats.Available);
            Assert.Equal(0.0, stats.AverageYear);
        }

        [Fact]
        public void GetStatistics_AfterBorrow_BorrowedCountIncreases()
        {
            var library = new Library();
            library.AddBook(new Book("Книга А", "Автор А", 2000));
            library.AddBook(new Book("Книга Б", "Автор Б", 2010));
            library.BorrowBook("Книга А");

            var stats = library.GetStatistics();

            Assert.Equal(2, stats.Total);
            Assert.Equal(1, stats.Available);
            Assert.Equal(1, stats.Borrowed);
        }
    }

    public class LibraryReturnBookTests
    {
        [Fact]
        public void ReturnBook_BorrowedBook_BecomesAvailableAgain()
        {
            var library = new Library();
            library.AddBook(new Book("Дюна", "Герберт", 1965));
            library.BorrowBook("Дюна");

            library.ReturnBook("Дюна");

            Assert.True(library.Books[0].IsAvailable);
        }

        [Fact]
        public void ReturnBook_NotBorrowed_ThrowsInvalidOperationException()
        {
            var library = new Library();
            library.AddBook(new Book("Дюна", "Герберт", 1965));

            Assert.Throws<InvalidOperationException>(() =>
                library.ReturnBook("Дюна"));
        }
    }
}

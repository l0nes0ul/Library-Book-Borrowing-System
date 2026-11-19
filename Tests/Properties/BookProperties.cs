using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Library_Book_Borrowing_System.Domain;
using Library_Book_Borrowing_System.Tests.Arbitraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library_Book_Borrowing_System.Tests.Properties
{
    public class BookProperties
    {
        // P8. Без винятків + When
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public Property Book_Constructor_ValidCopies_DoesNotThrow(string isbn, NonEmptyString title, int copies)
        {
            Action check = () =>
            {
                Action act = () => _ = new Book(isbn, title.Get, copies);
                act.Should().NotThrow();
            };

            return check.When(copies >= 1);
        }

        // P1. Туди й назад
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void Book_BorrowThenReturn_RestoresAvailableCopies(Book book)
        {
            int initialAvailable = book.AvailableCopies;

            book.Borrow();
            book.Return();

            book.AvailableCopies.Should().Be(initialAvailable);
        }

        // P5. Оракул
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void Service_BorrowBook_MatchesDirectBookBorrow(Book book, User user)
        {
            var bookForService = new Book(book.Isbn, book.Title, book.TotalCopies);
            var bookDirect = new Book(book.Isbn, book.Title, book.TotalCopies);

            var repo = new InMemoryLibraryRepository();
            repo.AddBook(bookForService);
            repo.AddUser(user);
            var service = new LibraryService(repo);

            service.BorrowBook(user.Id, bookForService.Isbn);
            bookDirect.Borrow();

            repo.GetBook(bookForService.Isbn).AvailableCopies.Should().Be(bookDirect.AvailableCopies);
        }
    }

}

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
    public class ErrorProperties
    {
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public Property Book_Constructor_NonPositiveCopies_ThrowsArgumentException(string isbn, NonEmptyString title, int copies)
        {
            Action check = () =>
            {
                Action act = () => _ = new Book(isbn, title.Get, copies);
                act.Should().Throw<ArgumentException>();
            };

            return check.When(copies < 1);
        }

        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void Book_Borrow_WhenNoCopiesAvailable_ThrowsInvalidOperationException(Book book)
        {
            for (int i = 0; i < book.TotalCopies; i++)
            {
                book.Borrow();
            }

            Action act = () => book.Borrow();
            act.Should().Throw<InvalidOperationException>();
        }

        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void Book_Return_WhenAllCopiesReturned_ThrowsInvalidOperationException(Book book)
        {
            Action act = () => book.Return();
            act.Should().Throw<InvalidOperationException>();
        }

        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public Property Service_ReturnBook_WhenNotBorrowed_ThrowsInvalidOperationException(Book book1, Book book2, User user, User otherUser)
        {
            Action check = () =>
            {
                var repo = new InMemoryLibraryRepository();
                repo.AddBook(book1);
                repo.AddBook(book2);
                repo.AddUser(user);
                repo.AddUser(otherUser);
                var service = new LibraryService(repo);

                service.BorrowBook(user.Id, book1.Isbn);
                service.BorrowBook(otherUser.Id, book2.Isbn);

                Action act = () => service.ReturnBook(user.Id, book2.Isbn);
                act.Should().Throw<InvalidOperationException>();
            };

            return check.When(book1.Isbn != book2.Isbn);
        }
    }

}

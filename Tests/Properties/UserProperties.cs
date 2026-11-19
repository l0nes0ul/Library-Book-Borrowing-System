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
    public class UserProperties
    {
        // P1. Туди й назад
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void User_BorrowThenReturn_RestoresEmptyBorrowedList(Book book, User user)
        {
            var repo = new InMemoryLibraryRepository();
            repo.AddBook(book);
            repo.AddUser(user);
            var service = new LibraryService(repo);

            service.BorrowBook(user.Id, book.Isbn);
            service.ReturnBook(user.Id, book.Isbn);

            var updatedUser = repo.GetUser(user.Id);
            updatedUser.BorrowedIsbns.Should().BeEmpty();
        }

        // P7. Легко перевірити
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void User_BorrowAndReturnCounts_MatchRemainingCopies(Book book, User user, NonNull<bool[]> actions)
        {
            var repo = new InMemoryLibraryRepository();
            repo.AddBook(book);
            repo.AddUser(user);
            var service = new LibraryService(repo);

            int borrows = 0;
            int returns = 0;

            foreach (var shouldBorrow in actions.Get)
            {
                var currentBook = repo.GetBook(book.Isbn);
                var currentUser = repo.GetUser(user.Id);

                if (shouldBorrow && currentBook.AvailableCopies > 0)
                {
                    service.BorrowBook(user.Id, book.Isbn);
                    borrows++;
                }
                else if (!shouldBorrow && currentUser.BorrowedIsbns.Contains(book.Isbn))
                {
                    service.ReturnBook(user.Id, book.Isbn);
                    returns++;
                }
            }

            var finalBook = repo.GetBook(book.Isbn);
            var finalUser = repo.GetUser(user.Id);

            finalBook.AvailableCopies.Should().Be(finalBook.TotalCopies - borrows + returns);
            finalUser.BorrowedIsbns.Should().HaveCount(borrows - returns);
        }

        // P4. Різні шляхи + When
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public Property Users_BorrowInDifferentOrder_SameAvailableCopies(Book book, User user1, User user2)
        {
            Action check = () =>
            {
                var repo1 = new InMemoryLibraryRepository();
                var bookPath1 = new Book(book.Isbn, book.Title, book.TotalCopies);
                var user1Path1 = new User(user1.Name);
                var user2Path1 = new User(user2.Name);
                repo1.AddBook(bookPath1);
                repo1.AddUser(user1Path1);
                repo1.AddUser(user2Path1);
                var service1 = new LibraryService(repo1);

                service1.BorrowBook(user1Path1.Id, bookPath1.Isbn);
                service1.BorrowBook(user2Path1.Id, bookPath1.Isbn);

                var repo2 = new InMemoryLibraryRepository();
                var bookPath2 = new Book(book.Isbn, book.Title, book.TotalCopies);
                var user1Path2 = new User(user1.Name);
                var user2Path2 = new User(user2.Name);
                repo2.AddBook(bookPath2);
                repo2.AddUser(user1Path2);
                repo2.AddUser(user2Path2);
                var service2 = new LibraryService(repo2);

                service2.BorrowBook(user2Path2.Id, bookPath2.Isbn);
                service2.BorrowBook(user1Path2.Id, bookPath2.Isbn);

                repo1.GetBook(bookPath1.Isbn).AvailableCopies.Should().Be(repo2.GetBook(bookPath2.Isbn).AvailableCopies);
                repo1.GetUser(user1Path1.Id).BorrowedIsbns.Should().BeEquivalentTo(repo2.GetUser(user1Path2.Id).BorrowedIsbns);
                repo1.GetUser(user2Path1.Id).BorrowedIsbns.Should().BeEquivalentTo(repo2.GetUser(user2Path2.Id).BorrowedIsbns);
            };

            return check.When(book.TotalCopies >= 2);
        }
    }

}

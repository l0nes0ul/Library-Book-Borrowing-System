using FluentAssertions;
using FsCheck;
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
    public class InvariantProperties
    {
        // P2. Інваріант
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void Book_AfterAnyValidOperations_AvailableCopiesWithinBounds(Book book, NonNull<bool[]> operations)
        {
            book.AvailableCopies.Should().BeInRange(0, book.TotalCopies);

            foreach (var shouldBorrow in operations.Get)
            {
                if (shouldBorrow && book.AvailableCopies > 0)
                {
                    book.Borrow();
                }
                else if (!shouldBorrow && book.AvailableCopies < book.TotalCopies)
                {
                    book.Return();
                }

                book.AvailableCopies.Should().BeInRange(0, book.TotalCopies);
            }
        }

        // P2. Інваріант
        [Property(Arbitrary = new[] {typeof(LibraryArbitraries)})]
        public void LibraryService_AfterAnyValidOperations_TotalCopiesConserved(Book book, User user, NonNull<bool[]> operations)
        {
            var repo = new InMemoryLibraryRepository();
            repo.AddBook(book);
            repo.AddUser(user);
            var service = new LibraryService(repo);

            (repo.GetBook(book.Isbn).AvailableCopies + repo.GetUser(user.Id).BorrowedIsbns.Count).Should().Be(book.TotalCopies);

            foreach (var shouldBorrow in operations.Get)
            {
                var currentBook = repo.GetBook(book.Isbn);
                var currentUser = repo.GetUser(user.Id);

                if (shouldBorrow && currentBook.AvailableCopies > 0)
                {
                    service.BorrowBook(user.Id, book.Isbn);
                }
                else if (!shouldBorrow && currentUser.BorrowedIsbns.Contains(book.Isbn))
                {
                    service.ReturnBook(user.Id, book.Isbn);
                }

                var updatedBook = repo.GetBook(book.Isbn);
                var updatedUser = repo.GetUser(user.Id);

                (updatedBook.AvailableCopies + updatedUser.BorrowedIsbns.Count).Should().Be(updatedBook.TotalCopies);
            }
        }
    }

}

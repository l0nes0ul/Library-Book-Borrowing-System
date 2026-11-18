using FsCheck;
using FsCheck.Fluent;
using Library_Book_Borrowing_System.Domain;

namespace Library_Book_Borrowing_System.Tests.Arbitraries
{
    public static class LibraryArbitraries
    {
        public static Arbitrary<string> Isbn()
        {
            var digitGen = Gen.Elements('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            
            var gen10 = digitGen.ArrayOf(10).Select(chars => new string(chars));
            var gen13 = digitGen.ArrayOf(13).Select(chars => new string(chars));
            
            return Arb.From(Gen.OneOf(gen10, gen13));
        }

        private static Gen<string> Title()
        {
            return Gen.Elements(
                "Clean Code",
                "Design Patterns",
                "Refactoring",
                "Domain-Driven Design",
                "C# in Depth"
            );
        }

        public static Arbitrary<Book> Book()
        {
            var bookGen = from isbn in Isbn().Generator
                          from title in Title()
                          from copies in Gen.Choose(1, 50) 
                          select new Book(isbn, title, copies);

            return Arb.From(bookGen);
        }

        public static Arbitrary<User> User()
        {
            var nameGen = Gen.Elements("Ahri", "Benjamin", "Courtney", "Donald", "Mina");
            
            var userGen = from name in nameGen
                          select new User(name);

            return Arb.From(userGen);
        }
    }
}

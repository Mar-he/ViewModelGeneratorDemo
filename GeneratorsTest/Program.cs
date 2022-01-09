using GeneratorsTest;


Book book = new() {
    Author = "John Doe",
    FaAuthor = 1,
    ISBN = "123-456-789",
    FaISBN = 1,
};

var vm = new BookVm(book);
Console.WriteLine(vm.Author.Value);
Console.WriteLine(vm.ISBN.Value);
Console.WriteLine(vm.Author.Fa);
Console.WriteLine(vm.ISBN.Fa);
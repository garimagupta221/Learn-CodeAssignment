class Book
{
    private int currentPage = 1;

    public string GetTitle()
    {
        return "A Great Book";
    }

    public string GetAuthor()
    {
        return "John Doe";
    }

    public void TurnPage()
    {
        currentPage++;
    }

    public string GetCurrentPage()
    {
        return "Current page content";
    }
}

class Library
{
    public string GetLocation(Book book)
    {
        return "Room & Shelf Number";
    }
}

interface IBookStorage
{
    void Save(Book book);
}

class FileBookStorage : IBookStorage
{
    public void Save(Book book)
    {
        string fileName = $"{book.GetTitle()} - {book.GetAuthor()}.txt";
        File.WriteAllText(fileName, "Book saved");
    }
}

interface IPrinter
{
    void PrintPage(string page);
}

class PlainTextPrinter : IPrinter
{
    public void PrintPage(string page)
    {
        Console.WriteLine(page);
    }
}

class HtmlPrinter : IPrinter
{
    public void PrintPage(string page)
    {
	Console.WriteLine(page);
    }
}

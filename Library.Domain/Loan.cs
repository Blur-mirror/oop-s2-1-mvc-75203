namespace Library.Domain;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }

    //This is just for convenience for now, not mapped to DB
    public bool IsOverdue =>
        ReturnedDate == null && DueDate < DateTime.Today;
}

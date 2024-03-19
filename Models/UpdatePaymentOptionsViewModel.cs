using System.ComponentModel.DataAnnotations;

public class UpdatePaymentOptionsViewModel
{
    [Display(Name = "Accept Credit Card")]
    public bool AcceptCreditCard { get; set; }

    [Display(Name = "Accept Cash")]
    public bool AcceptCash { get; set; }

    [Display(Name = "Accept Mobile Payment")]
    public bool AcceptMobilePayment { get; set; }
}

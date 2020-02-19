using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    [Table("BookingTB")]
    public class BookingTB
    {
        [Key]
        public int BookingID { get; set; }
        public string Name { get; set; }
        public int NoOfPeople { get; set; }
        public string FromDate { get; set; }
        public string FromTimer { get; set; }
        public string ToTimer { get; set; }
        public string ToDate { get; set; }
        public string S_address { get; set; }
        public string D_address { get; set; }
        public string Email_Id { get; set; }
        public string Contact_No { get; set; }
        public int PropertyId { get; set; }
        public int PropertyServiceId { get; set; }
        public int MeetingRoomId { get; set; }
        public bool IsFoodOrder { get; set; }
        public bool IsInternal { get; set; }
        public bool IsMVA { get; set; }
        public bool IsConfirmed { get; set; }
        public int C_Id { get; set; }
        public int Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string nameOfbook { get; set; }
        public int UserID { get; set; }
        public int SendMessageType { get;  set; }
        public string Note { get; set; }

        public string InvoMessage { get; set; }
        public List<BookingMessageView> MessageList { get; set; }

        public DateTime CreatedOn { get; set; }
        [NotMapped]
        public string Username { get; set; }
        [NotMapped]
        public string Carname { get; set; }
        [NotMapped]
        public string ModelName { get; set; }
        [NotMapped]
        public string BankName { get; set; }
        [NotMapped]
        public string Status { get; set; }
        public string FollowDate { get; set; }
        public int BookOrderName { get; set; }
        public int CompanyPayer { get; set; }
        public List<Food> foods { get; set; }
        public int FoodFormId { get; set; }
        public bool IsBooking { get; set; }
    }

    public partial class BookingMessageView: IValidatableObject
    {
        public int ID { get; set; }
        public string Type { get; set; }
        [Required]
        public string To{ get; set;}
        [Required]
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Attachments { get; set; }
        public string SendTime { get; set; }
        public int Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            string[] list = To.Split(';');
            foreach (string item in list)
            {
                if (!new EmailAddressAttribute().IsValid(item))
                {
                    yield return new ValidationResult( "email " + item + " is not valid.",new[] { "To" });
                }
            }
        }
    }

    public partial class Food
    {
        public int ID { get; set; }
        public int FoodId { get; set; }
        public string MainServiceId { get; set; }
        public decimal Qty { get; set; }
        public string ArticleId { get; set; }
        public int OrderHeadId { get; set; }
        public int OrderListId { get; set; }
        public string Tekst { get; set; }
        public decimal CostPrice { get; set; }
        public decimal CostTotal { get; set; }
        public string ServiceText { get; set; }
        public decimal Sum { get; set; }
        public decimal Price { get; set; }
        public int BookingID { get; set; }
        public bool Status { get; set; }
        public bool IsKitchenOrder { get; set; }
        public bool IsVatApply { get; set; }
        public string Time { get; set; }
        

    }

    public class PriceContent
    {
        public string unit { get; set; }
        public string unitText { get; set; }

        public decimal price1 { get; set; }
        public decimal price2 { get; set; }
        public decimal costPrice1 { get; set; }
        public decimal costPrice2 { get; set; }

        public string Article1 { get; set; }
        public string Article2 { get; set; }
        public int from { get; set; }
        public int to { get; set; }
    }
}

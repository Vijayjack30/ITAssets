using System.ComponentModel.DataAnnotations;
public class LoginVM
{
	[Required]
    public string? UserId { get; set; }
	[Required]
	[DataType(DataType.Password)]
    public string? Password { get; set; }
}

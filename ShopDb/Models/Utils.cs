namespace ShopDb.Models
{
    public class Utils
    {
        public static bool UserValidator(User user, HttpContext httpContext)
        {
            if (!user.FirstName.All(Char.IsLetter) || !user.LastName.All(Char.IsLetter))
            {
                httpContext.Session.SetString("UserValidator", "Etu- ja sukunimi voivat sisältää vain kirjaimia");
                return false;
            }
            if (user.FirstName.Length < 2 || user.LastName.Length < 2)
            {
                httpContext.Session.SetString("UserValidator", "Nimen pituuden tulee olla vähintään 2 merkkiä");
                return false;
            }
            if (user.FirstName.Length > 25 || user.LastName.Length > 25)
            {
                httpContext.Session.SetString("UserValidator", "Nimi voi olla korkeintaan 25 merkkiä pitkä");
                return false;
            }
            if (user.Phone.Length < 5 || user.Phone.Length > 15)
            {
                httpContext.Session.SetString("UserValidator", "Puhelinnumeron tulee olla vähintään 5 ja korkeintaan 15 merkkiä pitkä");
                return false;
            }
            if (!user.Email.Contains("@") || user.Email.EndsWith("."))
            {
                httpContext.Session.SetString("UserValidator", "Tarkista sähköpostiosoitteen kirjoitusasu. Sähköpostin tulee sisältää '@' merkki");
                return false;
            }
            if (user.Addresses.FirstOrDefault().Address1.Length < 5 || user.Addresses.FirstOrDefault().Address1.Length > 50)
            {
                httpContext.Session.SetString("UserValidator", "Osoitteen pituus ei voi olla alle 5 eikä yli 30 merkkiä");
                return false;
            }
            if (user.Addresses.FirstOrDefault().City.Length < 2 || user.Addresses.FirstOrDefault().City.Length > 35)
            {
                httpContext.Session.SetString("UserValidator", "Kaupungin nimen pituus ei voi olla alle 2 tai yli 35 merkkiä");
                return false;
            }
            if (!user.Addresses.FirstOrDefault().City.All(Char.IsLetter))
            {
                httpContext.Session.SetString("UserValidator", "Kaupungin nimi ei voi sisältää numeroita tai erikoismerkkejä");
                return false;
            }
            if (!user.Addresses.FirstOrDefault().PostalCode.All(Char.IsNumber))
            {
                httpContext.Session.SetString("UserValidator", "postinumero voi sisältää vain numeroita");
                return false;
            }
            if (user.Addresses.FirstOrDefault().PostalCode.Length != 5)
            {
                httpContext.Session.SetString("UserValidator", "Postinumeron tulee olla 5 merkkiä pitkä");
                return false;
            }
            else
                return true;
        }



    }
}

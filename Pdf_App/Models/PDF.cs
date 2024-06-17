namespace Pdf_App.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Pdf_Gpt4.Models
    {
        public class Pdf
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }

            public string AuthorId { get; set; }
            public string AuthorName { get; set; }

            public string PdfUrl { get; set; }



            public Pdf()
            {

            }

        }
    }

}

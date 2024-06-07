using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rpa.Curso.Domain.Models
{
    [Table("PlataformasDeEnsino")]
    public class KnowledgePlatform
    {
        [Key]
        public Guid Id { get; set; }

        [Column("Nome")]
        public string Name { get; set; }

        [Column("DataCriacao")]
        public DateTime CreatedDate { get; set; }
    }
}
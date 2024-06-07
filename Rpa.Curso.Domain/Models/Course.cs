using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rpa.Curso.Domain.Models
{
    [Table("Cursos")]
    public class Course
    {
        [Key]
        public Guid Id { get; set; }

        [Column("Titulo")]
        public string Title { get; set; }

        [ForeignKey("Instructor"), Column("ProfessorId")]
        public Guid? InstructorId { get; set; }

        [Column("CargaHoraria")]
        public int? Workload { get; set; }

        [Column("Descricao")]
        public string? Description { get; set; }

        [ForeignKey("KnowledgePlatform"), Column("PlataformaDeEnsinoId")]
        public Guid KnowledgePlatformId { get; set; }

        [Column("DataCriacao")]
        public DateTime CreatedDate { get; set; }

        [Column("Observacoes")]
        public string? Notes { get; set; }

        public Instructor? Instructor { get; set; }

        public KnowledgePlatform KnowledgePlatform { get; set; }
    }
}
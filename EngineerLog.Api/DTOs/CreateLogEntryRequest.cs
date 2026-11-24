namespace EngineerLog.Api.DTOs;

   public class CreateLogEntryRequest
   {
       public int ProjectId { get; set; }
       public int? CategoryId { get; set; } // ID выбранной категории (может быть null)
       public List<int> TagIds { get; set; } = new(); // Список ID выбранных тэгов
       public string Type { get; set; } = "Event"; // "Event" или "Problem"
       public string Title { get; set; } = string.Empty;
       public string Description { get; set; } = string.Empty;
       public string? Solution { get; set; }
   }

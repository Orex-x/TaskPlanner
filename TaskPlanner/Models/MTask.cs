


using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace TaskPlanner.Models;

public class MTask : ICloneable
{
    public int Id { get; set; }
    
    public string Code { get; set; }
    public string Title { get; set; }
        
    public string Description { get; set; }
        
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ssZ}", ApplyFormatInEditMode = true)]
    public DateTime Deadline { get; set; }
        
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm:ssZ}", ApplyFormatInEditMode = true)]
    public DateTime DateOfCreation { get; set; }
    
    public MTaskPriority MTaskPriority { get; set; }
    
    public MTaskStatus MTaskStatus { get; set; }
    
    public string AuthorEmail { get; set; }
    
    public bool Original { get; set; } 

    public List<MFile> PathToFiles { get; set; } = new List<MFile>();

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
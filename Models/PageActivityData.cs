using System;
using WorkLifeBalance.Models.Base;

namespace WorkLifeBalance.Models;

[Serializable]
public class PageActivityData : ActivityDataBase
{
    public override string Date { get; set; } = "26082026";
    public string? Url { get; set; } = ""; 
    public override string TimeSpent { get; set; } = "000000";
}
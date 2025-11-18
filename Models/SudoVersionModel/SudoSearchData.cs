public class SudoSearchData
{
    public string terminal_no { get; set; }
    public string status { get; set; }

    // สำหรับ paging
    public int maxRows { get; set; } = 50;
}
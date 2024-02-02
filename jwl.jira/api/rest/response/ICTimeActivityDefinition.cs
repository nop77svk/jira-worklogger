namespace jwl.jira.api.rest.response;

// 2do!
public class ICTimeActivityDefinition
{
    public ICTimeActivityDefinition(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}

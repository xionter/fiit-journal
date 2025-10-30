namespace FiitFlow.Domain;

public class Subject
{
    public readonly string Title;
    private string linkToSheet; //ну тут что надо стрингом либо чем либо другим
    
    // private (как мы храним баллы пока хз) points 

    public Subject(string title, string linkToSheet = "")
    {
        Title = title;
        this.linkToSheet = linkToSheet;
    }

    public void SetLink(string link, string groupId)
    {
        // сначала запрос в бд есть ли у нас табличка от этой группы если линки нет
        // linkToSheet = bd[subject.Title + groupId]
        // если есть то ставим ту которую передал пользователь
        
        linkToSheet = link;
    }

    public void GetPoints()
    {
        // ну там парсер чет делает
        // типо parser.Parse(linkToSheet)
        throw new NotImplementedException();
    }
    
}
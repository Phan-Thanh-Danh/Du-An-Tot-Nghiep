namespace Backend.Services.Applications;

public interface IApplicationTemplateValidator
{
    void Validate(string configurationJson);
}

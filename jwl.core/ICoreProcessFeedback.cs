namespace jwl.core;
using jwl.infra;

public interface ICoreProcessFeedback
    : IDisposable
{
    void FillJiraWithWorklogsStart();
    void FillJiraWithWorklogsSetTarget(int numberOfWorklogsToInsert, int numbeOfWorklogsToDelete);
    void FillJiraWithWorklogsProcess(MultiTaskProgress progress);
    void FillJiraWithWorklogsEnd();
    void NoExistingWorklogsToDelete();
    void NoFilesOnInput();
    void NoWorklogsToFill();
    void OverallProcessStart();
    void OverallProcessEnd();
    void PreloadAvailableWorklogTypesStart();
    void PreloadAvailableWorklogTypesEnd();
    void PreloadUserInfoStart(string userName);
    void PreloadUserInfoEnd();
    void ReadCsvInputStart();
    void ReadCsvInputSetTarget(int numberOfInputFiles);
    void ReadCsvInputProcess(MultiTaskProgress progress);
    void ReadCsvInputEnd();
    void RetrieveWorklogsForDeletionStart();
    void RetrieveWorklogsForDeletionSetTarget(int count);
    void RetrieveWorklogsForDeletionProcess(MultiTaskProgress progress);
    void RetrieveWorklogsForDeletionEnd();
}

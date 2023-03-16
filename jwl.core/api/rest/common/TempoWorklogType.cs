namespace jwl.core.api.rest.common;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum TempoWorklogType
{
    Other,
    Analysis,
    CodeReview,
    Consultation,
    Coordination,
    Documentation,
    FinalOffer,
    FSAnalysis,
    IAAnalysis,
    IndicativeOffer,
    Reporting,
    RFCReview,
    StudyTraining,
    SysAppProcessing,
    [EnumMember(Value = "Testing-FAT")]
    TestingFAT,
    [EnumMember(Value = "Testing-SIT")]
    TestingSIT,
    [EnumMember(Value = "SysApp-support")]
    SysAppSupport,
    BusinessAnalysis,
    Parametrization,
    Development,
    Installation,
    [EnumMember(Value = "Testing-ST")]
    TestingST,
    Meetings,
    ServiceDeskProcessing,
    [EnumMember(Value = "Testing-Support")]
    TestingSupport,
    [EnumMember(Value = "Testing-Regress")]
    TestingRegress,
    [EnumMember(Value = "Testing-UAT")]
    TestingUAT,
    Actions,
    [EnumMember(Value = "WorkingonIssue")]
    WorkingOnIssue
}

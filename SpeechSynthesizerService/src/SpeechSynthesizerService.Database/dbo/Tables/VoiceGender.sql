CREATE TABLE [dbo].[VoiceGender] (
    [VoiceGenderId] INT          NOT NULL,
    [Gender]        VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_VoiceGender] PRIMARY KEY CLUSTERED ([VoiceGenderId] ASC)
);


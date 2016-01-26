CREATE TABLE [dbo].[TTSMessage] (
    [TTSMessageId]  UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate]   DATETIME         NOT NULL,
    [ProcessedDate] DATETIME         NULL,
    [VoiceGenderId] INT              NOT NULL,
    [Message]       VARCHAR (1000)   NOT NULL,
    [Filename]      VARCHAR (200)    NULL,
    CONSTRAINT [PK_TTSMessage] PRIMARY KEY CLUSTERED ([TTSMessageId] ASC),
    CONSTRAINT [FK_TTSMessage_VoiceGender] FOREIGN KEY ([VoiceGenderId]) REFERENCES [dbo].[VoiceGender] ([VoiceGenderId])
);




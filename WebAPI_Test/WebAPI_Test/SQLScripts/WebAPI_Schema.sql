USE [hPay_Demo_HSA]
GO
/****** Object:  Table [dbo].[Client]    Script Date: 2/27/2017 2:27:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Client](
	[Id] [nvarchar](100) NOT NULL,
	[Secret] [nvarchar](500) NOT NULL,
	[Name] [nvarchar](500) NOT NULL,
	[ApplicationType] [int] NULL,
	[Active] [bit] NOT NULL,
	[RefreshTokenLifeTime] [int] NOT NULL,
	[AllowedOrigin] [nvarchar](500) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[RefreshToken]    Script Date: 2/27/2017 2:27:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefreshToken](
	[Id] [nvarchar](100) NOT NULL,
	[Subject] [nvarchar](2000) NOT NULL,
	[ClientId] [nvarchar](2000) NOT NULL,
	[IssuedUtc] [datetime] NULL,
	[ExpiresUtc] [datetime] NULL,
	[ProtectedTicket] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[Client] ([Id], [Secret], [Name], [ApplicationType], [Active], [RefreshTokenLifeTime], [AllowedOrigin]) VALUES (N'ConsoleApp', N'SECRET', N'Console Application', 1, 1, 2, N'*')
GO
INSERT [dbo].[Client] ([Id], [Secret], [Name], [ApplicationType], [Active], [RefreshTokenLifeTime], [AllowedOrigin]) VALUES (N'jQueryApp', N'SECRET', N'jQuery App', 0, 1, 10, N'*')
GO

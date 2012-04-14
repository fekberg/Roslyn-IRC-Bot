USE [RoslynIrcBot]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 04/15/2012 00:38:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [uniqueidentifier] NOT NULL,
	[Ident] [nvarchar](100) NULL,
	[UserLevel] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Namespaces]    Script Date: 04/15/2012 00:38:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Namespaces](
	[Id] [uniqueidentifier] NOT NULL,
	[Namespace] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Namespaces] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'4b5ab17f-e573-49aa-9b47-11b7ea39ce70', N'System.IO')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'e52a1f7a-d1b2-441b-b432-13b584f05229', N'System.Threading')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'363f8dfe-ec4e-4c51-b0fc-234b4a8db134', N'System.Text.RegularExpressions')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'827e51d2-1328-481b-bb04-3464232f04ea', N'System.Reflection.Emit')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'6a19a5f1-81ac-4047-a587-4069c759e316', N'System.Web')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'28c60ac4-073b-40cb-807a-548977a7a458', N'System.Diagnostics')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'ff309cbb-106e-4945-a961-6643a15669e7', N'System.Threading.Tasks')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'2e49c502-f525-45c3-88f3-9473de94ab01', N'System')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'3b5e36bf-0f64-4223-9fc0-9bacff9a415c', N'System.Text')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'802e865a-2f04-48c4-a68a-a11eb7715ed0', N'System.Linq')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'404a2269-924b-43ab-b24a-deb631d52c17', N'System.Collections.Generic')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'57bb5bc7-206c-4816-a0a4-e79f84def955', N'System.Reflection')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'89b8c2e8-041b-4c7e-a069-f82d77633f4c', N'System.Dynamic')
INSERT [dbo].[Namespaces] ([Id], [Namespace]) VALUES (N'a4c617ac-7cf3-443e-850f-f8b7a4a7762a', N'System.Net')
/****** Object:  Table [dbo].[Commands]    Script Date: 04/15/2012 00:38:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commands](
	[Id] [uniqueidentifier] NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[Command] [nvarchar](255) NOT NULL,
	[Posted] [datetime] NOT NULL,
 CONSTRAINT [PK_Commands] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Default [DF_Users_Id]    Script Date: 04/15/2012 00:38:42 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Id]  DEFAULT (newid()) FOR [Id]
GO
/****** Object:  Default [DF_Namespaces_Id]    Script Date: 04/15/2012 00:38:42 ******/
ALTER TABLE [dbo].[Namespaces] ADD  CONSTRAINT [DF_Namespaces_Id]  DEFAULT (newid()) FOR [Id]
GO
/****** Object:  Default [DF_Commands_Id]    Script Date: 04/15/2012 00:38:42 ******/
ALTER TABLE [dbo].[Commands] ADD  CONSTRAINT [DF_Commands_Id]  DEFAULT (newid()) FOR [Id]
GO

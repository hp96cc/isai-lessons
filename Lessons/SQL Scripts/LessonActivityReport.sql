USE [ISAI.Lessons.Web.Portal]
GO

/****** Object:  View [dbo].[LessonActivityReport]    Script Date: 9/7/2025 8:09:01 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   VIEW [dbo].[LessonActivityReport]
AS
SELECT TOP (100) PERCENT 

dbo.CustomerActivity.Id AS CustomerActivityId, 
dbo.Lesson.Id AS LessonId, 
dbo.Lesson.Name, 
dbo.LessonGroup.Name AS Expr1, 
dbo.CustomerActivity.StartDateTime, 
dbo.Customer.FirstName + ' ' + dbo.Customer.LastName AS CustomerName, 
dbo.Customer.Email AS CustomerEmail, 
dbo.CustomerDevice.Name AS CustomerDeviceName, 
dbo.Subscription.Name AS SubscriptionName,
dbo.SubscriptionCode.Code AS SubscriptionCode, 
dbo.SubscriptionCode.IssuedTo AS SubscriptionCodeIssuedTo


FROM dbo.CustomerActivity 
INNER JOIN dbo.Lesson ON dbo.Lesson.Id = dbo.CustomerActivity.LessonId 
INNER JOIN dbo.LessonGroup ON dbo.Lesson.LessonGroupId = dbo.LessonGroup.Id 
INNER JOIN dbo.CustomerDevice ON dbo.CustomerDevice.Id = dbo.CustomerActivity.CustomerDeviceId
INNER JOIN dbo.Customer ON dbo.CustomerDevice.CustomerId = dbo.Customer.Id
INNER JOIN dbo.Subscription ON dbo.Subscription.CustomerId = dbo.Customer.Id 
LEFT OUTER JOIN dbo.SubscriptionCode ON dbo.Subscription.Id = dbo.SubscriptionCode.SubscriptionId
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'LessonActivityReport', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Lesson"
            Begin Extent = 
               Top = 87
               Left = 430
               Bottom = 447
               Right = 687
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LessonGroup"
            Begin Extent = 
               Top = 13
               Left = 90
               Bottom = 210
               Right = 364
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Customer"
            Begin Extent = 
               Top = 279
               Left = 1039
               Bottom = 476
               Right = 1368
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CustomerActivity"
            Begin Extent = 
               Top = 13
               Left = 792
               Bottom = 210
               Right = 1041
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Subscription"
            Begin Extent = 
               Top = 153
               Left = 1365
               Bottom = 350
               Right = 1630
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CustomerDevice"
            Begin Extent = 
               Top = 216
               Left = 57
               Bottom = 413
               Right = 284
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SubscriptionCode"
            Begin Extent = 
               Top = 216
               Left = 744
               Bottom = 413
               Right = 1001
      ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LessonActivityReport'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Lesson"
            Begin Extent = 
               Top = 87
               Left = 430
               Bottom = 447
               Right = 687
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LessonGroup"
            Begin Extent = 
               Top = 13
               Left = 90
               Bottom = 210
               Right = 364
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Customer"
            Begin Extent = 
               Top = 279
               Left = 1039
               Bottom = 476
               Right = 1368
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CustomerActivity"
            Begin Extent = 
               Top = 13
               Left = 792
               Bottom = 210
               Right = 1041
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Subscription"
            Begin Extent = 
               Top = 153
               Left = 1365
               Bottom = 350
               Right = 1630
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CustomerDevice"
            Begin Extent = 
               Top = 216
               Left = 57
               Bottom = 413
               Right = 284
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SubscriptionCode"
            Begin Extent = 
               Top = 216
               Left = 744
               Bottom = 413
               Right = 1001
      ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LessonActivityReport'
END
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPane2' , N'SCHEMA',N'dbo', N'VIEW',N'LessonActivityReport', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'      End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LessonActivityReport'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_DiagramPane2', @value=N'      End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LessonActivityReport'
END
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'LessonActivityReport', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LessonActivityReport'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'LessonActivityReport'
END
GO



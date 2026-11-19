IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF SCHEMA_ID(N'nodoscomerciales') IS NULL EXEC(N'CREATE SCHEMA [nodoscomerciales];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF SCHEMA_ID(N'tiendas') IS NULL EXEC(N'CREATE SCHEMA [tiendas];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF SCHEMA_ID(N'auth') IS NULL EXEC(N'CREATE SCHEMA [auth];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[MarcasTienda] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_MarcasTienda] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [nodoscomerciales].[NivelesJerarquia] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Profundidad] int NOT NULL,
        CONSTRAINT [PK_NivelesJerarquia] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NivelesJerarquia_Profundidad] CHECK ([Profundidad] >= 1)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[Regionales] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Regionales] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[ResponsablesVisuales] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_ResponsablesVisuales] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [auth].[Roles] (
        [Id] int NOT NULL IDENTITY,
        [Descripcion] nvarchar(200) NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[TiposEspacio] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [Categoria] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_TiposEspacio] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TiposEspacio_Categoria] CHECK ([Categoria] IN ('Mobiliario','Perimetral'))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[TiposTienda] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_TiposTienda] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [auth].[Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [NombreUsuario] nvarchar(50) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [HashContrasena] nvarchar(256) NOT NULL,
        [SelloSeguridad] uniqueidentifier NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IntentosFallidos] int NOT NULL DEFAULT 0,
        [BloqueadoHastaUtc] datetime2 NULL,
        [FechaCreacionUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Usuarios_IntentosFallidos] CHECK ([IntentosFallidos] >= 0)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [nodoscomerciales].[ConceptosComerciales] (
        [Id] int NOT NULL IDENTITY,
        [NivelJerarquiaId] int NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [UnidadesPorEspacio] int NULL,
        CONSTRAINT [PK_ConceptosComerciales] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ConceptosComerciales_UnidadesPorEspacio] CHECK ([UnidadesPorEspacio] IS NULL OR [UnidadesPorEspacio] > 0),
        CONSTRAINT [FK_ConceptosComerciales_NivelesJerarquia_NivelJerarquiaId] FOREIGN KEY ([NivelJerarquiaId]) REFERENCES [nodoscomerciales].[NivelesJerarquia] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[Tiendas] (
        [Id] int NOT NULL IDENTITY,
        [CodigoHanna] int NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [MarcaTiendaId] int NOT NULL,
        [RegionalId] int NOT NULL,
        [TipoTiendaId] int NOT NULL,
        [ResponsableVisualId] int NULL,
        CONSTRAINT [PK_Tiendas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tiendas_MarcasTienda_MarcaTiendaId] FOREIGN KEY ([MarcaTiendaId]) REFERENCES [tiendas].[MarcasTienda] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tiendas_Regionales_RegionalId] FOREIGN KEY ([RegionalId]) REFERENCES [tiendas].[Regionales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tiendas_ResponsablesVisuales_ResponsableVisualId] FOREIGN KEY ([ResponsableVisualId]) REFERENCES [tiendas].[ResponsablesVisuales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tiendas_TiposTienda_TipoTiendaId] FOREIGN KEY ([TipoTiendaId]) REFERENCES [tiendas].[TiposTienda] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [auth].[SesionesToken] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [Jti] uniqueidentifier NOT NULL,
        [HashTokenRefresco] nvarchar(128) NOT NULL,
        [FechaEmisionUtc] datetime2 NOT NULL,
        [FechaExpiracionUtc] datetime2 NOT NULL,
        [FechaRevocacionUtc] datetime2 NULL,
        [MotivoRevocacion] nvarchar(30) NULL,
        [ReemplazadaPorSesionId] uniqueidentifier NULL,
        [DireccionIp] nvarchar(45) NULL,
        [AgenteUsuario] nvarchar(256) NULL,
        CONSTRAINT [PK_SesionesToken] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_SesionesToken_Fechas] CHECK ([FechaExpiracionUtc] > [FechaEmisionUtc]),
        CONSTRAINT [CK_SesionesToken_MotivoRevocacion] CHECK ([MotivoRevocacion] IS NULL OR [MotivoRevocacion] IN ('Rotacion','CierreSesion','CambioContrasena','ReusoDetectado','Administrativa')),
        CONSTRAINT [CK_SesionesToken_Reemplazo] CHECK ([ReemplazadaPorSesionId] IS NULL OR [MotivoRevocacion] = 'Rotacion'),
        CONSTRAINT [CK_SesionesToken_Revocacion] CHECK (([FechaRevocacionUtc] IS NULL AND [MotivoRevocacion] IS NULL) OR ([FechaRevocacionUtc] IS NOT NULL AND [MotivoRevocacion] IS NOT NULL)),
        CONSTRAINT [FK_SesionesToken_SesionesToken_ReemplazadaPorSesionId] FOREIGN KEY ([ReemplazadaPorSesionId]) REFERENCES [auth].[SesionesToken] ([Id]),
        CONSTRAINT [FK_SesionesToken_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [auth].[Usuarios] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [auth].[UsuariosRoles] (
        [UsuarioId] uniqueidentifier NOT NULL,
        [RolId] int NOT NULL,
        CONSTRAINT [PK_UsuariosRoles] PRIMARY KEY ([UsuarioId], [RolId]),
        CONSTRAINT [FK_UsuariosRoles_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [auth].[Roles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UsuariosRoles_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [auth].[Usuarios] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [nodoscomerciales].[NodosComerciales] (
        [Id] int NOT NULL IDENTITY,
        [NodoPadreId] int NULL,
        [ConceptoComercialId] int NOT NULL,
        [PorcentajeParticipacion] decimal(18,6) NOT NULL,
        CONSTRAINT [PK_NodosComerciales] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_NodosComerciales_PorcentajeParticipacion] CHECK ([PorcentajeParticipacion] > 0 AND [PorcentajeParticipacion] <= 1),
        CONSTRAINT [FK_NodosComerciales_ConceptosComerciales_ConceptoComercialId] FOREIGN KEY ([ConceptoComercialId]) REFERENCES [nodoscomerciales].[ConceptosComerciales] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NodosComerciales_NodosComerciales_NodoPadreId] FOREIGN KEY ([NodoPadreId]) REFERENCES [nodoscomerciales].[NodosComerciales] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE TABLE [tiendas].[EspaciosTienda] (
        [TiendaId] int NOT NULL,
        [TipoEspacioId] int NOT NULL,
        [CantidadEspacios] int NOT NULL,
        CONSTRAINT [PK_EspaciosTienda] PRIMARY KEY ([TiendaId], [TipoEspacioId]),
        CONSTRAINT [CK_EspaciosTienda_CantidadEspacios] CHECK ([CantidadEspacios] >= 0),
        CONSTRAINT [FK_EspaciosTienda_Tiendas_TiendaId] FOREIGN KEY ([TiendaId]) REFERENCES [tiendas].[Tiendas] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EspaciosTienda_TiposEspacio_TipoEspacioId] FOREIGN KEY ([TipoEspacioId]) REFERENCES [tiendas].[TiposEspacio] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[MarcasTienda]'))
        SET IDENTITY_INSERT [tiendas].[MarcasTienda] ON;
    EXEC(N'INSERT INTO [tiendas].[MarcasTienda] ([Id], [Nombre])
    VALUES (1, N''YAHAD''),
    (2, N''LILI PINK''),
    (3, N''YOI'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[MarcasTienda]'))
        SET IDENTITY_INSERT [tiendas].[MarcasTienda] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre', N'Profundidad') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[NivelesJerarquia]'))
        SET IDENTITY_INSERT [nodoscomerciales].[NivelesJerarquia] ON;
    EXEC(N'INSERT INTO [nodoscomerciales].[NivelesJerarquia] ([Id], [Nombre], [Profundidad])
    VALUES (1, N''Unidad de Negocio'', 1),
    (2, N''Categoría'', 2),
    (3, N''Marca de Producto'', 3),
    (4, N''Segmento'', 4),
    (5, N''Subcategoría'', 5),
    (6, N''Perfil'', 6),
    (7, N''Tipo de Producto'', 7)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre', N'Profundidad') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[NivelesJerarquia]'))
        SET IDENTITY_INSERT [nodoscomerciales].[NivelesJerarquia] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[Regionales]'))
        SET IDENTITY_INSERT [tiendas].[Regionales] ON;
    EXEC(N'INSERT INTO [tiendas].[Regionales] ([Id], [Nombre])
    VALUES (1, N''BOGOTA''),
    (2, N''ANTIOQUIA'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[Regionales]'))
        SET IDENTITY_INSERT [tiendas].[Regionales] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[auth].[Roles]'))
        SET IDENTITY_INSERT [auth].[Roles] ON;
    EXEC(N'INSERT INTO [auth].[Roles] ([Id], [Descripcion], [Nombre])
    VALUES (1, N''Crea tiendas y administra usuarios, roles y catalogos'', N''admin''),
    (2, N''Administra tiendas existentes (actualiza sus datos y espacios); no crea tiendas'', N''admin_tienda''),
    (3, N''Parametriza el arbol comercial (conceptos, nodos y porcentajes)'', N''Analista''),
    (4, N''Solo lectura de calculos y reportes'', N''Consulta'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[auth].[Roles]'))
        SET IDENTITY_INSERT [auth].[Roles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Categoria', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[TiposEspacio]'))
        SET IDENTITY_INSERT [tiendas].[TiposEspacio] ON;
    EXEC(N'INSERT INTO [tiendas].[TiposEspacio] ([Id], [Categoria], [Nombre])
    VALUES (1, N''Mobiliario'', N''Isla''),
    (2, N''Mobiliario'', N''Casa David''),
    (3, N''Mobiliario'', N''Mesa''),
    (4, N''Mobiliario'', N''Góndola''),
    (5, N''Mobiliario'', N''Mesa Central''),
    (6, N''Mobiliario'', N''Exhibidor''),
    (7, N''Perimetral'', N''Perimetral'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Categoria', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[TiposEspacio]'))
        SET IDENTITY_INSERT [tiendas].[TiposEspacio] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[TiposTienda]'))
        SET IDENTITY_INSERT [tiendas].[TiposTienda] ON;
    EXEC(N'INSERT INTO [tiendas].[TiposTienda] ([Id], [Nombre])
    VALUES (1, N''MIXTA''),
    (2, N''MONOMARCA'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[tiendas].[TiposTienda]'))
        SET IDENTITY_INSERT [tiendas].[TiposTienda] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'NivelJerarquiaId', N'Nombre', N'UnidadesPorEspacio') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[ConceptosComerciales]'))
        SET IDENTITY_INSERT [nodoscomerciales].[ConceptosComerciales] ON;
    EXEC(N'INSERT INTO [nodoscomerciales].[ConceptosComerciales] ([Id], [NivelJerarquiaId], [Nombre], [UnidadesPorEspacio])
    VALUES (1, 1, N''BELLEZA Y BIENESTAR'', NULL),
    (2, 2, N''BEAUTY'', NULL),
    (3, 3, N''LILI BEAUTY'', NULL),
    (4, 3, N''KARITE'', NULL),
    (5, 4, N''GIRLS'', NULL),
    (6, 5, N''MAKEUP'', NULL),
    (7, 6, N''LABIOS'', NULL),
    (8, 6, N''OJOS'', NULL),
    (9, 6, N''ROSTRO'', NULL),
    (10, 7, N''LABIALES'', 660),
    (11, 7, N''BRILLO LABIOS'', 540),
    (12, 7, N''PESTAÑINA'', 288),
    (13, 7, N''DELINEADOR'', 420),
    (14, 7, N''BASE / POLVOS'', 310),
    (15, 7, N''BÁLSAMO LABIAL'', 600)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'NivelJerarquiaId', N'Nombre', N'UnidadesPorEspacio') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[ConceptosComerciales]'))
        SET IDENTITY_INSERT [nodoscomerciales].[ConceptosComerciales] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CodigoHanna', N'MarcaTiendaId', N'Nombre', N'RegionalId', N'ResponsableVisualId', N'TipoTiendaId') AND [object_id] = OBJECT_ID(N'[tiendas].[Tiendas]'))
        SET IDENTITY_INSERT [tiendas].[Tiendas] ON;
    EXEC(N'INSERT INTO [tiendas].[Tiendas] ([Id], [CodigoHanna], [MarcaTiendaId], [Nombre], [RegionalId], [ResponsableVisualId], [TipoTiendaId])
    VALUES (1, 5503, 1, N''LILI EN YOI YAHAD TOBERIN BOGOTA'', 1, NULL, 1),
    (2, 1001, 2, N''LILI PINK UNICENTRO BOGOTA'', 1, NULL, 2),
    (3, 2045, 3, N''YOI VIVA ENVIGADO'', 2, NULL, 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CodigoHanna', N'MarcaTiendaId', N'Nombre', N'RegionalId', N'ResponsableVisualId', N'TipoTiendaId') AND [object_id] = OBJECT_ID(N'[tiendas].[Tiendas]'))
        SET IDENTITY_INSERT [tiendas].[Tiendas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TiendaId', N'TipoEspacioId', N'CantidadEspacios') AND [object_id] = OBJECT_ID(N'[tiendas].[EspaciosTienda]'))
        SET IDENTITY_INSERT [tiendas].[EspaciosTienda] ON;
    EXEC(N'INSERT INTO [tiendas].[EspaciosTienda] ([TiendaId], [TipoEspacioId], [CantidadEspacios])
    VALUES (1, 1, 7),
    (1, 2, 4),
    (1, 3, 4),
    (1, 7, 7),
    (2, 1, 6),
    (2, 4, 10),
    (2, 7, 14),
    (3, 5, 8),
    (3, 6, 6),
    (3, 7, 6)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'TiendaId', N'TipoEspacioId', N'CantidadEspacios') AND [object_id] = OBJECT_ID(N'[tiendas].[EspaciosTienda]'))
        SET IDENTITY_INSERT [tiendas].[EspaciosTienda] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConceptoComercialId', N'NodoPadreId', N'PorcentajeParticipacion') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[NodosComerciales]'))
        SET IDENTITY_INSERT [nodoscomerciales].[NodosComerciales] ON;
    EXEC(N'INSERT INTO [nodoscomerciales].[NodosComerciales] ([Id], [ConceptoComercialId], [NodoPadreId], [PorcentajeParticipacion])
    VALUES (1, 1, NULL, 1.0),
    (2, 2, 1, 0.3),
    (3, 3, 2, 0.94),
    (4, 4, 2, 0.06),
    (5, 5, 3, 1.0),
    (15, 5, 4, 1.0),
    (6, 6, 5, 1.0),
    (16, 6, 15, 1.0),
    (7, 7, 6, 0.4),
    (8, 8, 6, 0.45),
    (9, 9, 6, 0.15),
    (17, 7, 16, 1.0),
    (10, 10, 7, 0.5),
    (11, 11, 7, 0.5),
    (12, 12, 8, 0.6),
    (13, 13, 8, 0.4),
    (14, 14, 9, 1.0),
    (18, 15, 17, 1.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConceptoComercialId', N'NodoPadreId', N'PorcentajeParticipacion') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[NodosComerciales]'))
        SET IDENTITY_INSERT [nodoscomerciales].[NodosComerciales] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_ConceptosComerciales_NivelJerarquiaId_Nombre] ON [nodoscomerciales].[ConceptosComerciales] ([NivelJerarquiaId], [Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_EspaciosTienda_TipoEspacioId] ON [tiendas].[EspaciosTienda] ([TipoEspacioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_MarcasTienda_Nombre] ON [tiendas].[MarcasTienda] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_NivelesJerarquia_Nombre] ON [nodoscomerciales].[NivelesJerarquia] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_NivelesJerarquia_Profundidad] ON [nodoscomerciales].[NivelesJerarquia] ([Profundidad]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_NodosComerciales_ConceptoComercialId] ON [nodoscomerciales].[NodosComerciales] ([ConceptoComercialId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UQ_NodosComerciales_NodoPadreId_ConceptoComercialId] ON [nodoscomerciales].[NodosComerciales] ([NodoPadreId], [ConceptoComercialId]) WHERE [NodoPadreId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Regionales_Nombre] ON [tiendas].[Regionales] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_ResponsablesVisuales_Nombre] ON [tiendas].[ResponsablesVisuales] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Roles_Nombre] ON [auth].[Roles] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_SesionesToken_ReemplazadaPorSesionId] ON [auth].[SesionesToken] ([ReemplazadaPorSesionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_SesionesToken_UsuarioId_FechaExpiracionUtc] ON [auth].[SesionesToken] ([UsuarioId], [FechaExpiracionUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_SesionesToken_HashTokenRefresco] ON [auth].[SesionesToken] ([HashTokenRefresco]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_SesionesToken_Jti] ON [auth].[SesionesToken] ([Jti]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Tiendas_MarcaTiendaId] ON [tiendas].[Tiendas] ([MarcaTiendaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Tiendas_RegionalId_MarcaTiendaId] ON [tiendas].[Tiendas] ([RegionalId], [MarcaTiendaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Tiendas_ResponsableVisualId] ON [tiendas].[Tiendas] ([ResponsableVisualId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_Tiendas_TipoTiendaId] ON [tiendas].[Tiendas] ([TipoTiendaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Tiendas_CodigoHanna] ON [tiendas].[Tiendas] ([CodigoHanna]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_TiposEspacio_Nombre] ON [tiendas].[TiposEspacio] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_TiposTienda_Nombre] ON [tiendas].[TiposTienda] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Usuarios_Correo] ON [auth].[Usuarios] ([Correo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Usuarios_NombreUsuario] ON [auth].[Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    CREATE INDEX [IX_UsuariosRoles_RolId] ON [auth].[UsuariosRoles] ([RolId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119033056_MigracionInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261119033056_MigracionInicial', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    ALTER TABLE [nodoscomerciales].[NodosComerciales] ADD [TiendaId] int NOT NULL DEFAULT 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'UPDATE [nodoscomerciales].[NodosComerciales] SET [TiendaId] = 1
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConceptoComercialId', N'NodoPadreId', N'PorcentajeParticipacion', N'TiendaId') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[NodosComerciales]'))
        SET IDENTITY_INSERT [nodoscomerciales].[NodosComerciales] ON;
    EXEC(N'INSERT INTO [nodoscomerciales].[NodosComerciales] ([Id], [ConceptoComercialId], [NodoPadreId], [PorcentajeParticipacion], [TiendaId])
    VALUES (2001, 1, NULL, 1.0, 2),
    (3001, 1, NULL, 1.0, 3),
    (2002, 2, 2001, 0.3, 2),
    (3002, 2, 3001, 0.3, 3),
    (2003, 3, 2002, 0.94, 2),
    (2004, 4, 2002, 0.06, 2),
    (3003, 3, 3002, 0.94, 3),
    (3004, 4, 3002, 0.06, 3),
    (2005, 5, 2003, 1.0, 2),
    (2015, 5, 2004, 1.0, 2),
    (3005, 5, 3003, 1.0, 3),
    (3015, 5, 3004, 1.0, 3),
    (2006, 6, 2005, 1.0, 2),
    (2016, 6, 2015, 1.0, 2),
    (3006, 6, 3005, 1.0, 3),
    (3016, 6, 3015, 1.0, 3),
    (2007, 7, 2006, 0.4, 2),
    (2008, 8, 2006, 0.45, 2),
    (2009, 9, 2006, 0.15, 2),
    (2017, 7, 2016, 1.0, 2),
    (3007, 7, 3006, 0.4, 3),
    (3008, 8, 3006, 0.45, 3),
    (3009, 9, 3006, 0.15, 3),
    (3017, 7, 3016, 1.0, 3),
    (2010, 10, 2007, 0.5, 2),
    (2011, 11, 2007, 0.5, 2),
    (2012, 12, 2008, 0.6, 2),
    (2013, 13, 2008, 0.4, 2),
    (2014, 14, 2009, 1.0, 2),
    (2018, 15, 2017, 1.0, 2),
    (3010, 10, 3007, 0.5, 3),
    (3011, 11, 3007, 0.5, 3),
    (3012, 12, 3008, 0.6, 3),
    (3013, 13, 3008, 0.4, 3),
    (3014, 14, 3009, 1.0, 3),
    (3018, 15, 3017, 1.0, 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConceptoComercialId', N'NodoPadreId', N'PorcentajeParticipacion', N'TiendaId') AND [object_id] = OBJECT_ID(N'[nodoscomerciales].[NodosComerciales]'))
        SET IDENTITY_INSERT [nodoscomerciales].[NodosComerciales] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    CREATE INDEX [IX_NodosComerciales_TiendaId] ON [nodoscomerciales].[NodosComerciales] ([TiendaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz] ON [nodoscomerciales].[NodosComerciales] ([TiendaId], [ConceptoComercialId]) WHERE [NodoPadreId] IS NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    ALTER TABLE [nodoscomerciales].[NodosComerciales] ADD CONSTRAINT [FK_NodosComerciales_Tiendas_TiendaId] FOREIGN KEY ([TiendaId]) REFERENCES [tiendas].[Tiendas] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119192632_ArbolPorTienda'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261119192632_ArbolPorTienda', N'8.0.11');
END;
GO

COMMIT;
GO


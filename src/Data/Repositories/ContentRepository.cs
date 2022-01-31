using Dapper;
using System.Text;

namespace Diov.Data;

public class ContentRepository : IContentRepository
{
    private const string DeleteStatement =
        @"DELETE FROM [Content]
        WHERE [Path] = @Path;";
    private const string InsertStatement =
        @"INSERT INTO [Content]
        (
            [Body],
            [IsIndexed],
            [Name],
            [Path],
            [PublishedDateTime],
            [Summary]
        )
        OUTPUT INSERTED.[Id]
        VALUES
        (
            @Body,
            @IsIndexed,
            @Name,
            @Path,
            @PublishedDateTime,
            @Summary
        );";
    private const string SelectStatement =
        @"SELECT
            [Body],
            [Id],
            [IsIndexed],
            [Name],
            [Path],
            [PublishedDateTime],
            [Summary]
        FROM [Content]
        WHERE 1 = 1";
    private const string SelectStatementIsIndexedPredicate =
        @" AND [IsIndexed] = @IsIndexed";
    private const string SelectStatementOrderByClause =
        @" ORDER BY [PublishedDateTime] DESC
        OFFSET @Skip ROWS
        FETCH NEXT @Take ROWS ONLY";
    private const string SelectStatementPathPredicate =
        @" AND [Path] = @Path";
    private const string SelectTotalCountStatement =
        @"SELECT COUNT(*)
        FROM [Content]
        WHERE 1 = 1";
    private const string UpdateStatement =
        @"UPDATE Content
        SET
            [Body] = @Body,
            [IsIndexed] = @IsIndexed,
            [Name] = @Name,
            [Path] = @Path,
            [PublishedDateTime] = @PublishedDateTime,
            [Summary] = @Summary
        WHERE [Id] = @Id;";

    public ContentRepository(
        IDbConnectionFactory dbConnectionFactory)
    {
        DbConnectionFactory = dbConnectionFactory;
    }

    public IDbConnectionFactory DbConnectionFactory { get; }

    public async Task<int> AddContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        using var sqlConnection = await DbConnectionFactory
            .GetSqlConnectionAsync(cancellationToken);
        using var sqlTransaction = await sqlConnection
            .BeginTransactionAsync(cancellationToken);
        try
        {
            var id = (await sqlConnection
                .QueryAsync<int>(
                    InsertStatement,
                    content,
                    sqlTransaction))
                .FirstOrDefault();

            await sqlTransaction.CommitAsync(
                cancellationToken);

            return id;
        }
        catch
        {
            await sqlTransaction.RollbackAsync(
                cancellationToken);
            throw;
        }
    }

    public async Task DeleteContentAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        using var sqlConnection = await DbConnectionFactory
            .GetSqlConnectionAsync(cancellationToken);
        using var sqlTransaction = await sqlConnection
            .BeginTransactionAsync(cancellationToken);
        try
        {
            await sqlConnection.ExecuteAsync(
                DeleteStatement,
                new { Path = path, },
                sqlTransaction);

            await sqlTransaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await sqlTransaction.RollbackAsync(
                cancellationToken);
            throw;
        }
    }

    public async Task<Content?> GetContentAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var selectStatementBuilder = new StringBuilder(
            SelectStatement);
        selectStatementBuilder.Append(
            SelectStatementPathPredicate);
        selectStatementBuilder.Append(';');

        using var sqlConnection = await DbConnectionFactory
            .GetSqlConnectionAsync(cancellationToken);
        return await sqlConnection
            .QuerySingleOrDefaultAsync<Content?>(
                selectStatementBuilder.ToString(),
                new
                {
                    Path = path,
                });
    }

    public async Task<SearchResponse<Content>> SearchContentAsync(
        ContentSearchRequest contentSearchRequest,
        int skip = 0,
        int take = 5,
        CancellationToken cancellationToken = default)
    {
        var selectStatementBuilder = new StringBuilder(
            SelectStatement);
        var selectTotalCountStatementBuilder = new StringBuilder(
            SelectTotalCountStatement);

        if (contentSearchRequest.IsIndexed.HasValue)
        {
            selectStatementBuilder.Append(
                SelectStatementIsIndexedPredicate);
            selectTotalCountStatementBuilder.Append(
                SelectStatementIsIndexedPredicate);
        }
        if (!string.IsNullOrWhiteSpace(contentSearchRequest.Path))
        {
            selectStatementBuilder.Append(
                SelectStatementPathPredicate);
            selectTotalCountStatementBuilder.Append(
                SelectStatementPathPredicate);
        }

        selectStatementBuilder.Append(
            SelectStatementOrderByClause);

        selectStatementBuilder.Append(';');
        selectTotalCountStatementBuilder.Append(';');

        using var sqlConnection = await DbConnectionFactory
            .GetSqlConnectionAsync(cancellationToken);
        var contents = await sqlConnection
            .QueryAsync<Content>(
                selectStatementBuilder.ToString(),
                new
                {
                    contentSearchRequest.IsIndexed,
                    contentSearchRequest.Path,
                    Skip = skip,
                    Take = take,
                });

        var totalCount = await sqlConnection
            .ExecuteScalarAsync<int>(
                selectTotalCountStatementBuilder.ToString(),
                new
                {
                    contentSearchRequest.IsIndexed,
                    contentSearchRequest.Path,
                });

        return new SearchResponse<Content>
        {
            Items = contents,
            Skip = skip,
            Take = take,
            TotalCount = totalCount,
        };
    }

    public async Task UpdateContentAsync(
        Content content,
        CancellationToken cancellationToken = default)
    {
        using var sqlConnection = await DbConnectionFactory
            .GetSqlConnectionAsync(cancellationToken);
        using var sqlTransaction = await sqlConnection
            .BeginTransactionAsync(cancellationToken);
        try
        {
            var recordsAffected = await sqlConnection
                .ExecuteAsync(
                    UpdateStatement,
                    content,
                    sqlTransaction);
            if (recordsAffected != 1)
            {
                throw new ArgumentException(
                    "Content not found",
                    nameof(content));
            }

            await sqlTransaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await sqlTransaction.RollbackAsync(
                cancellationToken);
            throw;
        }
    }
}
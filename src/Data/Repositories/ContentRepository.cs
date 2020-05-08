using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diov.Data
{
    public class ContentRepository : IContentRepository
    {
        private const string DeleteStatement =
            @"DELETE FROM Content
            WHERE Id = @Id;";
        private const string InsertStatement =
            @"INSERT INTO Content
            (
                Body,
                IsIndexed,
                Name,
                Path,
                PublishedDateTime,
                Summary
            )
            OUTPUT INSERTED.Id
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
                Body,
                Id,
                IsIndexed,
                Name,
                Path,
                PublishedDateTime,
                Summary
            FROM Content
            WHERE 1 = 1";
        private const string SelectStatementIsIndexedPredicate =
            @" AND IsIndexed = @IsIndexed";
        private const string SelectStatementOrderByClause =
            @" ORDER BY PublishedDateTime DESC
            OFFSET @Index ROWS
            FETCH NEXT @PageSize ROWS ONLY";
        private const string SelectStatementPathPredicate =
            @" AND Path = @Path";
        private const string SelectTotalCountStatement =
            @"SELECT COUNT(*)
            FROM Content
            WHERE 1 = 1";
        private const string UpdateStatement =
            @"UPDATE Content
            SET
                Body = @Body,
                IsIndexed = @IsIndexed,
                Name = @Name,
                Path = @Path,
                PublishedDateTime = @PublishedDateTime,
                Summary = @Summary
            WHERE Id = @Id;";

        public ContentRepository(IDbConnectionFactory dbConnectionFactory)
        {
            DbConnectionFactory = dbConnectionFactory ??
                throw new ArgumentNullException(nameof(dbConnectionFactory));
        }

        public IDbConnectionFactory DbConnectionFactory { get; }

        public async Task<int> AddContentAsync(Content content)
        {
            using (var sqlConnection =
                await DbConnectionFactory.GetSqlConnectionAsync())
            {
                using (var sqlTransaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        var id = (await sqlConnection.QueryAsync<int>(
                            InsertStatement,
                            content,
                            sqlTransaction))
                            .FirstOrDefault();

                        sqlTransaction.Commit();

                        return id;
                    }
                    catch
                    {
                        sqlTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteContentAsync(int id)
        {
            using (var sqlConnection =
                await DbConnectionFactory.GetSqlConnectionAsync())
            {
                await sqlConnection.ExecuteAsync(
                    DeleteStatement,
                    new { Id = id, });
            }
        }

        public async Task<SearchResponse<Content>> SearchContentsAsync(
            ContentSearchRequest contentSearchRequest,
            int pageIndex = 0,
            int pageSize = 5)
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

            selectStatementBuilder.Append(";");
            selectTotalCountStatementBuilder.Append(";");

            var contents = default(IEnumerable<Content>);
            var totalCount = 0;
            using (var sqlConnection =
                await DbConnectionFactory.GetSqlConnectionAsync())
            {
                contents = await sqlConnection
                    .QueryAsync<Content>(
                        selectStatementBuilder.ToString(),
                        new
                        {
                            Index = pageIndex * pageSize,
                            IsIndexed = contentSearchRequest.IsIndexed,
                            Path = contentSearchRequest.Path,
                            PageSize = pageSize,
                        });

                totalCount = await sqlConnection
                    .ExecuteScalarAsync<int>(
                        selectTotalCountStatementBuilder.ToString(),
                        new
                        {
                            IsIndexed = contentSearchRequest.IsIndexed,
                            Path = contentSearchRequest.Path,
                        });
            }

            return new SearchResponse<Content>
            {
                Items = contents,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        public async Task UpdateContentAsync(Content content)
        {
            using (var sqlConnection =
                await DbConnectionFactory.GetSqlConnectionAsync())
            {
                using (var sqlTransaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        var recordsAffected = await sqlConnection.ExecuteAsync(
                            UpdateStatement,
                            content,
                            sqlTransaction);
                        if (recordsAffected != 1)
                        {
                            throw new ArgumentException(nameof(content));
                        }

                        sqlTransaction.Commit();
                    }
                    catch
                    {
                        sqlTransaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
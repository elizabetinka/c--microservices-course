using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Task3.Persistence.Migrations;

#pragma warning disable SA1649

[Migration(version: 14101024, description: "Initial migration")]
public class InitialMigration : IMigration
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """ 
                           create table products
                           (
                               product_id    bigint primary key generated always as identity,
                           
                               product_name  text  not null,
                               product_price money not null
                           );
                           
                           create type order_state as enum ('created', 'processing', 'completed', 'cancelled');
                           
                           create table orders
                           (
                               order_id         bigint primary key generated always as identity,
                           
                               order_state      order_state              not null,
                               order_created_at timestamp with time zone not null,
                               order_created_by text                     not null
                           );
                           
                           create table order_items
                           (
                               order_item_id       bigint primary key generated always as identity,
                               order_id            bigint  not null references orders (order_id),
                               product_id          bigint  not null references products (product_id),
                           
                               order_item_quantity int     not null,
                               order_item_deleted  boolean not null
                           );
                           
                           create type order_history_item_kind as enum ('created', 'item_added', 'item_removed', 'state_changed');
                           
                           create table order_history
                           (
                               order_history_item_id         bigint primary key generated always as identity,
                               order_id                      bigint                   not null references orders (order_id),
                           
                               order_history_item_created_at timestamp with time zone not null,
                               order_history_item_kind       order_history_item_kind  not null,
                               order_history_item_payload    jsonb                    not null
                           );
                           """,
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = """
                           drop table products;
                           drop table orders;
                           drop table order_items;
                           drop table order_history;
                           drop type order_state;
                           drop type order_history_item_kind;
                           """,
        });
    }

    public string ConnectionString => throw new NotSupportedException();
}
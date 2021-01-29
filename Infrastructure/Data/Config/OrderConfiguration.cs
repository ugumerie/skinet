using System;
using Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(o => o.ShipToAdress, a => {
                a.WithOwner();
            });

            //convert the enum property to string
            builder.Property(s => s.Status)
                .HasConversion(
                    o => o.ToString(),  //convert to string and saved in the database
                    o => (OrderStatus) Enum.Parse(typeof(OrderStatus), o) //converted back to enum out of the database
                );

            builder.HasMany(o => o.OrderItems).WithOne().OnDelete(DeleteBehavior.Cascade);
        }
    }
}
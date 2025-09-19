using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasLazu.AspNet.Authentication.Password.Domain.Entities;

namespace MasLazu.AspNet.Authentication.Password.EfCore.Configurations;

public class UserPasswordLoginConfiguration : IEntityTypeConfiguration<UserPasswordLogin>
{
    public void Configure(EntityTypeBuilder<UserPasswordLogin> builder)
    {
        builder.HasKey(upl => upl.Id);

        builder.Property(upl => upl.UserLoginMethodId)
            .IsRequired();

        builder.Property(upl => upl.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(upl => upl.UserLoginMethodId)
            .IsUnique();
    }
}

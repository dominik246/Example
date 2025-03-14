﻿// <auto-generated />
using System;
using Example.AuthApi.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Example.AuthApi.Database.Migrations
{
    [DbContext(typeof(AuthDbContext))]
    [Migration("20250314162314_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Example.AuthApi.Database.Models.Group", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsAdminGroup")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeletable")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Groups");

                    b.HasData(
                        new
                        {
                            Id = new Guid("01958cf3-d0bb-7509-80b6-4a15b06ad596"),
                            CreatedBy = new Guid("00000000-0000-0000-0000-000000000000"),
                            DateCreated = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            IsAdminGroup = true,
                            IsDeletable = false,
                            Name = "Admin",
                            RowVersion = 0u
                        });
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("character varying(320)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDisabled")
                        .HasColumnType("boolean");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = new Guid("019587b0-955a-72ad-a5eb-09442f53a85d"),
                            DateCreated = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Email = "admin@example.hr",
                            EmailConfirmed = true,
                            IsDisabled = false,
                            PasswordHash = "AQAAAAIAAYagAAAAELyuFf3ULbO89ih8KD4YGU4NN5+o0cg7H1XmrO+aUULcr3CuN5RfyM+ZcbYJDJNx4A==",
                            RowVersion = 0u
                        });
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserEmailConfirm", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserEmailConfirms");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserGroup", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("UserId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("UserGroups");

                    b.HasData(
                        new
                        {
                            UserId = new Guid("019587b0-955a-72ad-a5eb-09442f53a85d"),
                            GroupId = new Guid("01958cf3-d0bb-7509-80b6-4a15b06ad596"),
                            DateCreated = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            RowVersion = 0u
                        });
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserPasswordRestore", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<string>("SecurityCode")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserPasswordRestores");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserToken", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("AccessExpiry")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<DateTimeOffset>("DateCreated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("RefreshExpiry")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserEmailConfirm", b =>
                {
                    b.HasOne("Example.AuthApi.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserGroup", b =>
                {
                    b.HasOne("Example.AuthApi.Database.Models.Group", "Group")
                        .WithMany("UserGroups")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Example.AuthApi.Database.Models.User", "User")
                        .WithMany("UserGroups")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserPasswordRestore", b =>
                {
                    b.HasOne("Example.AuthApi.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.UserToken", b =>
                {
                    b.HasOne("Example.AuthApi.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.Group", b =>
                {
                    b.Navigation("UserGroups");
                });

            modelBuilder.Entity("Example.AuthApi.Database.Models.User", b =>
                {
                    b.Navigation("UserGroups");
                });
#pragma warning restore 612, 618
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ChessWPF.Data;

#nullable disable

namespace ChessWPF.Migrations
{
    [DbContext(typeof(ChessDbContext))]
    partial class ChessDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("ChessWPF.Models.GameRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BlackPlayer")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Event")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<string>("FinalFen")
                        .HasColumnType("TEXT");

                    b.Property<string>("InitialFen")
                        .HasColumnType("TEXT");

                    b.Property<int>("MoveCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("PlayedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("PgnNotation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Result")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Round")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Site")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<string>("WhitePlayer")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("BlackPlayer");

                    b.HasIndex("WhitePlayer");

                    b.ToTable("GameRecords");
                });
#pragma warning restore 612, 618
        }
    }
}

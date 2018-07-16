﻿// <auto-generated />
using System;
using BuddyBot.Repository.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuddyBot.Repository.Migrations
{
    [DbContext(typeof(BuddyBotDbContext))]
    partial class BuddyBotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BuddyBot.Repository.Models.City", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double?>("CoordinatesLatitude");

                    b.Property<double?>("CoordinatesLongitude");

                    b.Property<string>("Country");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CoordinatesLatitude", "CoordinatesLongitude");

                    b.ToTable("City");
                });

            modelBuilder.Entity("BuddyBot.Repository.Models.Coordinate", b =>
                {
                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.HasKey("Latitude", "Longitude");

                    b.ToTable("Coordinate");
                });

            modelBuilder.Entity("BuddyBot.Repository.Models.WeatherConditionResponse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Condition");

                    b.Property<string>("Group");

                    b.Property<string>("MappedConditionResponse");

                    b.HasKey("Id");

                    b.ToTable("WeatherConditionResponse");

                    b.HasData(
                        new { Id = 200, Condition = "Rain", Group = "Thunda", MappedConditionResponse = "Rain " }
                    );
                });

            modelBuilder.Entity("BuddyBot.Repository.Models.City", b =>
                {
                    b.HasOne("BuddyBot.Repository.Models.Coordinate", "Coordinates")
                        .WithMany()
                        .HasForeignKey("CoordinatesLatitude", "CoordinatesLongitude");
                });
#pragma warning restore 612, 618
        }
    }
}

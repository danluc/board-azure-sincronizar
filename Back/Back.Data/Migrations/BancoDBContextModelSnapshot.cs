﻿// <auto-generated />
using Back.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Back.Data.Migrations
{
    [DbContext(typeof(BancoDBContext))]
    partial class BancoDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("Back.Dominio.Models.Configuracao", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("Integer");

                    b.Property<int>("Dia")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("Int")
                        .HasDefaultValue(1);

                    b.Property<string>("HoraCron")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasDefaultValue("11:30");

                    b.HasKey("Id");

                    b.ToTable("CONFIGURACOES");
                });

            modelBuilder.Entity("Back.Dominio.Models.Conta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("Integer");

                    b.Property<string>("AreaPath");

                    b.Property<string>("NomeProjeto")
                        .HasMaxLength(255);

                    b.Property<string>("NomeUsuario")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<bool>("Principal")
                        .HasColumnType("Bit");

                    b.Property<string>("ProjetoId")
                        .HasMaxLength(255);

                    b.Property<string>("TimeId")
                        .HasMaxLength(255);

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("UrlCorporacao")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.ToTable("CONTAS");
                });
#pragma warning restore 612, 618
        }
    }
}

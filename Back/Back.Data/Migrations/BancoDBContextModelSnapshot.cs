﻿// <auto-generated />
using System;
using Back.Data.Context;
using Back.Dominio.Enum;
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
                        .HasColumnType("Int");

                    b.Property<string>("HoraCron")
                        .IsRequired()
                        .HasMaxLength(10);

                    b.HasKey("Id");

                    b.ToTable("CONFIGURACOES");
                });

            modelBuilder.Entity("Back.Dominio.Models.Conta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("Integer");

                    b.Property<string>("AreaPath");

                    b.Property<string>("NomeUsuario")
                        .HasMaxLength(255);

                    b.Property<bool>("Principal")
                        .HasColumnType("Bit");

                    b.Property<string>("ProjetoId")
                        .HasMaxLength(255);

                    b.Property<string>("ProjetoNome")
                        .HasMaxLength(255);

                    b.Property<string>("Sprint")
                        .HasMaxLength(255);

                    b.Property<string>("TimeId")
                        .HasMaxLength(255);

                    b.Property<string>("TimeNome")
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

            modelBuilder.Entity("Back.Dominio.Models.Sincronizar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("Integer");

                    b.Property<DateTime?>("DataFim");

                    b.Property<DateTime>("DataInicio")
                        .HasColumnType("DateTime");

                    b.Property<EStatusSincronizar>("Status")
                        .HasColumnType("Int");

                    b.HasKey("Id");

                    b.ToTable("SINCRONIZACOES");
                });
#pragma warning restore 612, 618
        }
    }
}

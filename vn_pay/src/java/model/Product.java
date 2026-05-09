/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package model;

import java.time.LocalDate;

/**
 *
 * @author admin
 */
public class Product {
    private int Id, Stock;
    private String Title, Author, Description;
    private LocalDate PublishedYearDate, CreatedAt;

    public Product() {
    }

    public int getId() {
        return Id;
    }

    public void setId(int Id) {
        this.Id = Id;
    }

    public int getStock() {
        return Stock;
    }

    public void setStock(int Stock) {
        this.Stock = Stock;
    }

    public String getTitle() {
        return Title;
    }

    public void setTitle(String Title) {
        this.Title = Title;
    }

    public String getAuthor() {
        return Author;
    }

    public void setAuthor(String Author) {
        this.Author = Author;
    }

    public String getDescription() {
        return Description;
    }

    public void setDescription(String Description) {
        this.Description = Description;
    }

    public LocalDate getPublishedYearDate() {
        return PublishedYearDate;
    }

    public void setPublishedYearDate(LocalDate PublishedYearDate) {
        this.PublishedYearDate = PublishedYearDate;
    }

    public LocalDate getCreatedAt() {
        return CreatedAt;
    }

    public void setCreatedAt(LocalDate CreatedAt) {
        this.CreatedAt = CreatedAt;
    }

    @Override
    public String toString() {
        return "Product{" + "Id=" + Id + ", Stock=" + Stock + ", Title=" + Title + ", Author=" + Author + ", Description=" + Description + ", PublishedYearDate=" + PublishedYearDate + ", CreatedAt=" + CreatedAt + '}';
    }
}

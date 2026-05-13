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
public class Order {
    private int Id, UserID;
    private double TotalAmount;
    private String status;
    private LocalDate OrderDate;

    public Order() {
    }

    public Order(int Id, int UserID, double TotalAmount, LocalDate OrderDate) {
        this.Id = Id;
        this.UserID = UserID;
        this.TotalAmount = TotalAmount;
        this.OrderDate = OrderDate;
    }

    public String getStatus() {
        return status;
    }

    public void setStatus(String status) {
        this.status = status;
    }

    public int getId() {
        return Id;
    }

    public void setId(int Id) {
        this.Id = Id;
    }

    public int getUserID() {
        return UserID;
    }

    public void setUserID(int UserID) {
        this.UserID = UserID;
    }

    public double getTotalAmount() {
        return TotalAmount;
    }

    public void setTotalAmount(double TotalAmount) {
        this.TotalAmount = TotalAmount;
    }

    public LocalDate getOrderDate() {
        return OrderDate;
    }

    public void setOrderDate(LocalDate OrderDate) {
        this.OrderDate = OrderDate;
    }

    @Override
    public String toString() {
        return "Order{" + "Id=" + Id + ", UserID=" + UserID + ", TotalAmount=" + TotalAmount + ", OrderDate=" + OrderDate + '}';
    }
}

/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package dao;

import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import model.Order;

/**
 *
 * @author admin
 */
public class OrderDao extends DBContext{
    
    public int insertOrder(Order order) {
        String sql = "INSERT INTO [dbo].[Orders]\n"
                + "           ([UserID]\n"
                + "           ,[TotalAmount])\n"
                + "     VALUES(?, ?)";
        try {
            PreparedStatement st = connection.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS);
            st.setInt(1, order.getUserID());
            st.setDouble(2, order.getTotalAmount());
            int affectedRows = st.executeUpdate();
            if (affectedRows == 0) {
                throw new SQLException("Creating payment failed, no rows affected.");
            }
            try ( ResultSet generatedKeys = st.getGeneratedKeys()) {
                if (generatedKeys.next()) {
                    return generatedKeys.getInt(1);
                } else {
                    throw new SQLException("Creating payment failed, no ID obtained.");
                }
            }
        } catch (SQLException e) {
            System.out.println(e);
            return -1;
        }
    }
    
    public boolean updateOrderStatus(Order order) {
        String sql = "UPDATE [dbo].[Orders]\n"
                + "   SET [Status] = ?\n"
                + " WHERE Id = ?";
        try {
            PreparedStatement st = connection.prepareStatement(sql);
            st.setString(1, order.getStatus());
            st.setInt(2, order.getId());
            return st.executeUpdate() > 0;
        } catch (SQLException ex) {
            System.out.println(ex);
        }
        return false;
    }
}

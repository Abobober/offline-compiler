import java.util.Scanner;

public class Program{
      
    public static void main (String args[]){
          
        Scanner scanner = new Scanner(System.in);

        System.out.println("Temperature Converter");
        System.out.println("Фаренгейты в Цельсии:");

        System.out.print("Введите значение температуры в Фаренгейтах: ");
        int fahrenheit = scanner.nextInt();
        System.out.println(fahrenheit + " градусов по Фаренгейту = " + fahrenheitToCelsius(fahrenheit) + " градусов по Цельсию.");

        System.out.println("Цельсии в Фаренгейты:");
        
        System.out.print("Введите значение температуры в Цельсиях: ");
        int celsius = scanner.nextInt();
        System.out.println(celsius + " градусов Цельсию = " + celsiusToFahrenheit(celsius) + " градусов по Фаренгейту.");
        
            
    }
    
    public static double fahrenheitToCelsius(double fahrenheit){
        double celsius = 5/9 * (fahrenheit - 32);
        return celsius;
    }

    public static double celsiusToFahrenheit(double celsius){
        double fahrenheit = celsius * 9/5 + 32;
        return fahrenheit;
    }
}
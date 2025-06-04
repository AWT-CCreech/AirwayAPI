namespace AirwayAPI.Application;

public class BadRequestException(string msg) : Exception(msg) { }
public class NotFoundException(string msg) : Exception(msg) { }
public class UnauthorizedException(string msg) : Exception(msg) { }
import { Injectable } from '@angular/core';

/**
 * Service for managing data storage and retrieval in the browser's localStorage.
 * Provides methods to store and retrieve string values with error handling.
 */
@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {
  private readonly storageKey = 'contracts365_instanceId';

  /**
   * Stores a string value in localStorage under the specified key.
   * @param value The string value to store (e.g., instanceId).
   */
  storeInstanceId(value?: string): void {
    try {
        value ? localStorage.setItem(this.storageKey, value) : localStorage.removeItem(this.storageKey);
      
    } catch (error) {
      console.error('Error storing to localStorage:', error);
    }
  }

  /**
   * Retrieves a string value from localStorage for the specified key.
   * @returns The stored string value, or an empty string if not found or an error occurs.
   */
  getInstanceId(): string {
    try {
      return localStorage.getItem(this.storageKey) || '';
    } catch (error) {
      console.error('Error retrieving from localStorage:', error);
      return '';
    }
  }

  /**
   * Removes the stored value from localStorage for the specified key.
   */
  clearInstanceId(): void {
    try {
      localStorage.removeItem(this.storageKey);
    } catch (error) {
      console.error('Error clearing localStorage:', error);
    }
  }
}
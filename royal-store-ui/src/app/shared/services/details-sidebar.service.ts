import { Injectable, signal, Type } from '@angular/core';

// Define a estrutura do conteúdo dinâmico
export interface DrawerContent {
  component: Type<any>; // O tipo de classe do componente (ex: PetDetailsContentComponent)
  data?: any; // Os dados que serão passados para o componente injetado
}

@Injectable({
  providedIn: 'root',
})
export class DetailsSidebarService {
  contentToRender = signal<DrawerContent | null>(null);

  // Sinal para controlar a visibilidade da "casca" do drawer
  sidebarVisible = signal(false);

  /**
   * Abre o drawer injetando um componente
   * @param component O componente (a classe) a ser carregado.
   * @param data Os dados (o registro) a ser exibido.
   */
  openDrawer(component: Type<any>) {
    this.contentToRender.set({ component });
    this.sidebarVisible.set(true);
  }

  closeDrawer() {
    this.sidebarVisible.set(false);
    // O conteúdo será limpo pelo efeito no componente shell após o fechamento
  }

  navigate() {
    this.sidebarVisible.set(true);
  }
}

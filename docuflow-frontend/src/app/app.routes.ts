// import { Routes } from '@angular/router';
// import { LoginComponent } from './pages/login/login.component';
// import { RegisterComponent } from './pages/register/register.component';
// import { DashboardComponent } from './pages/dashboard/dashboard.component';
//
// export const routes: Routes = [
//   { path: '', redirectTo: 'login', pathMatch: 'full' },
//   { path: 'login', component: LoginComponent },
//   { path: 'register', component: RegisterComponent },
//   { path: 'dashboard', component: DashboardComponent }
// ];
import { Routes } from '@angular/router';
import {LayoutComponent} from "./layout/layout.component";
import {DashboardComponent} from "./pages/dashboard/dashboard.component";
import {UsersComponent} from "./pages/users/users.component";
import {DocumentsComponent} from "./pages/documents/documents.component";

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: 'dashboard', component: DashboardComponent },
      //{ path: 'profile', component: ProfileComponent },
       { path: 'documents', component: DocumentsComponent },
      //{ path: 'notifications', component: NotificationsComponent },
      //{ path: 'todo', component: TodoComponent },
      { path: 'users', component: UsersComponent }
    ]
  },
  {
    path: 'documents/:id',
    loadComponent: () => import('./pages/document-viewer.component').then(m => m.DocumentViewerComponent)
  },

  /*{
    path: 'documents/:id',
    loadComponent: () =>
      import('./pages/document-viewer.component').then(
        (m) => m.DocumentViewerComponent
      )
  }*/
];
